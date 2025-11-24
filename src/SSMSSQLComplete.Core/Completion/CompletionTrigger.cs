namespace SSMSSQLComplete.Core.Completion
{
    public enum CompletionTriggerKind
    {
        Invoked,        // Manually triggered (Ctrl+Space)
        TriggerCharacter, // Triggered by typing a character (., space)
        TriggerForIncompleteCompletions
    }

    public class CompletionTrigger
    {
        public CompletionTriggerKind Kind { get; set; }
        public char? TriggerCharacter { get; set; }

        public CompletionTrigger(CompletionTriggerKind kind, char? triggerCharacter = null)
        {
            Kind = kind;
            TriggerCharacter = triggerCharacter;
        }

        public static CompletionTrigger Invoked => new CompletionTrigger(CompletionTriggerKind.Invoked);

        public static CompletionTrigger ForCharacter(char c) =>
            new CompletionTrigger(CompletionTriggerKind.TriggerCharacter, c);
    }
}
