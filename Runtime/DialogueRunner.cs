using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Moths.Dialogues
{
    public class DialogueRunner
    {
        private Dialogue _currentDialogue;
        private DialogueElement _currentElement;
        private int _seqLineIndex;
        private Dictionary<string, DialogueRunner> _nestedRunners = new();
        private Stack<DialogueRunner> _nestedRunnersPool = new();

        public Dialogue Current => _currentDialogue;

        public event Action<Dialogue> OnStarted;
        public event Action<DialogueLine> OnLine;
        public event Action<IReadOnlyList<DialogueChoice>> OnChoices;
        public event Action OnAction;
        public event Action<DialogueOutput> OnOutput;
        public event Action OnEnded;

        private void Clear()
        {
            _currentDialogue = null;
            _currentElement = default;
            _seqLineIndex = 0;
            _nestedRunners.Clear();
            _nestedRunnersPool.Clear();
            OnStarted = null;
            OnLine = null;
            OnChoices = null;
            OnAction = null;
            OnOutput = null;
            OnEnded = null;
        }

        public void Start(Dialogue dialogue)
        {
            _currentDialogue = dialogue;
            _seqLineIndex = 0;
            _nestedRunners.Clear();

            _currentElement = dialogue.Start();

            OnStarted?.Invoke(_currentDialogue);

            ProcessCurrentElement().Forget();
        }

        public void Next(int choiceIndex = -1)
        {
            if (!Current) return;

            if (_currentElement.Type == DialogueElement.ElementType.None)
            {
                End();
                return;
            }

            switch (_currentElement.Type)
            {
                case DialogueElement.ElementType.Sequence:
                    if (choiceIndex >= 0) return;

                    var sequence = (DialogueSequence)_currentElement;
                    _seqLineIndex++;
                    if (sequence.IsSequenceComplete(_seqLineIndex))
                    {
                        _seqLineIndex = 0;
                        _currentElement = _currentDialogue.Next(_currentElement);
                    }
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Choices:
                    if (choiceIndex < 0) return;
                    
                    var choices = (DialogueChoices)_currentElement;
                    var choice = choices.Choices[choiceIndex];
                    
                    if (choices.IsProcedural)
                    {
                        choices.ProcessChoice(choice);
                        _currentElement = _currentDialogue.Next(choices.ProceduralGuid);
                    }
                    else
                    {
                        _currentElement = _currentDialogue.Next(choice.Guid);
                    }
                    
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Dialogue:
                    var nested = (DialogueNested)_currentElement;
                    nested.Next(_nestedRunners[nested.Guid], choiceIndex);
                    break;
            }
        }

        public void End()
        {
            _currentDialogue = null;
            _currentElement = default;
            OnEnded?.Invoke();
        }

        private DialogueRunner GetNestedRunner(string guid)
        {
            void NestedRunnerStartedCallback(string guid, Dialogue dialogue)
            {

            }

            void NestedRunnerOutputCallback(string guid, DialogueOutput output)
            {
                _currentElement = _currentDialogue.Next(output.Guid);
                ProcessCurrentElement().Forget();
            }

            void NestedRunnerEndedCallback(string guid)
            {
                _nestedRunners[guid].Clear();
                _nestedRunnersPool.Push(_nestedRunners[guid]);
                _nestedRunners.Remove(guid);
            }

            DialogueRunner runner = null;
            if (!_nestedRunnersPool.TryPop(out runner)) runner = new();

            runner.OnLine += OnLine;
            runner.OnChoices += OnChoices;
            runner.OnAction += OnAction;
            
            runner.OnStarted += dialogue => NestedRunnerStartedCallback(guid, dialogue);
            runner.OnOutput += output => NestedRunnerOutputCallback(guid, output);
            runner.OnEnded += () => NestedRunnerEndedCallback(guid);

            return runner;
        }

        private async UniTask ProcessCurrentElement()
        {
            switch (_currentElement.Type)
            {
                case DialogueElement.ElementType.None:
                    End();
                    return;


                case DialogueElement.ElementType.Sequence:
                    var sequence = (DialogueSequence)_currentElement;
                    OnLine?.Invoke(sequence.Lines[_seqLineIndex]);
                    break;

                case DialogueElement.ElementType.Choices:
                    var choices = (DialogueChoices)_currentElement;
                    OnChoices?.Invoke(choices.Choices);
                    break;

                case DialogueElement.ElementType.Action:
                    var action = (DialogueAction)_currentElement;
                    OnAction?.Invoke();
                    await action.Execute();
                    _currentElement = _currentDialogue.Next(_currentElement);
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Condition:
                    var condition = (DialogueCondition)_currentElement;
                    string nextGuid = condition.Check() ? condition.TrueOutputGuid : condition.FalseOutputGuid;
                    _currentElement = _currentDialogue.Next(nextGuid);
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Jump:
                    _currentElement = _currentDialogue.Next(_currentElement);
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Random:
                    var random = (DialogueRandom)_currentElement;
                    string randomNextGuid = random.GetRandomOutputGuid();
                    _currentElement = _currentDialogue.Next(randomNextGuid);
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Switch:
                    var dialogueSwitch = (DialogueSwitch)_currentElement;
                    string switchNextGuid = dialogueSwitch.Evaluate();
                    _currentElement = _currentDialogue.Next(switchNextGuid);
                    ProcessCurrentElement().Forget();
                    break;

                case DialogueElement.ElementType.Dialogue:
                    var dialogueNested = (DialogueNested)_currentElement;
                    if (!_nestedRunners.ContainsKey(dialogueNested.Guid))
                    {
                        _nestedRunners[dialogueNested.Guid] = GetNestedRunner(dialogueNested.Guid);
                        _nestedRunners[dialogueNested.Guid].Start(dialogueNested.Dialogue);
                    }
                    break;

                case DialogueElement.ElementType.Output:
                    var output = (DialogueOutput)_currentElement;
                    OnOutput?.Invoke(output);
                    End();
                    break;
            }
        }
    }
}