namespace Moths.Dialogues
{
    public interface IDialogueCondition
    {
        string Description { get; }
        bool Check();
    }
}