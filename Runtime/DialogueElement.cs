namespace Moths.Dialogues
{
    public struct DialogueElement
    {
        public enum ElementType
        {
            None,
            Sequence,
            Choices,
            Action,
            Output,
            Condition,
            Jump,
        };
        
        private string _guid;
        private string _outputGuid;
        private DialogueSequence _sequence;
        private DialogueChoices _choices;
        private DialogueAction _action;
        private DialogueOutput _output;
        private DialogueCondition _condition;
        private DialogueJump _jump;

        public string Guid => _guid;
        public string OutputGuid => _outputGuid;
        public ElementType Type { get; private set; }

        public DialogueElement(string guid, string outputGuid, ElementType type)
        {
            this = default;
            Type = type;
            _guid = guid;
            _outputGuid = outputGuid;
        }

        public DialogueElement(DialogueSequence sequence) : this(sequence.Guid, sequence.OutputGuid, ElementType.Sequence) => _sequence = sequence;
        public DialogueElement(DialogueChoices choices) : this(choices.Guid, string.Empty, ElementType.Choices) => _choices = choices;
        public DialogueElement(DialogueAction action) : this(action.Guid, action.OutputGuid, ElementType.Action) => _action = action;
        public DialogueElement(DialogueOutput output) : this(output.Guid, string.Empty, ElementType.Output) => _output = output;
        public DialogueElement(DialogueCondition condition) : this(condition.Guid, string.Empty, ElementType.Condition) => _condition = condition;
        public DialogueElement(DialogueJump jump) : this(jump.Guid, jump.OutputGuid, ElementType.Jump) => _jump = jump;

        public static implicit operator DialogueElement(DialogueSequence element) => new(element);
        public static implicit operator DialogueElement(DialogueChoices element) => new(element);
        public static implicit operator DialogueElement(DialogueAction element) => new(element);
        public static implicit operator DialogueElement(DialogueOutput element) => new(element);
        public static implicit operator DialogueElement(DialogueCondition element) => new(element);
        public static implicit operator DialogueElement(DialogueJump element) => new(element);

        public static explicit operator DialogueSequence(DialogueElement element) => element._sequence;
        public static explicit operator DialogueChoices(DialogueElement element) => element._choices;
        public static explicit operator DialogueAction(DialogueElement element) => element._action;
        public static explicit operator DialogueOutput(DialogueElement element) => element._output;
        public static explicit operator DialogueCondition(DialogueElement element) => element._condition;
        public static explicit operator DialogueJump(DialogueElement element) => element._jump;
    }
}