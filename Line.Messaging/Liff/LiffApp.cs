namespace Line.Messaging.Liff;

public class LiffApp(string liffId, View view)
{
    public string LiffId { get; } = liffId;
    public View View { get; } = view;
}