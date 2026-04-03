namespace CoreTester.Tests;

[UnitTest]
public class ReplaceVariablesTest
{
    [UnitTest]
    public void SimpleTest()
    {
        var text = "d {Name} {Age} k";
        var found = text.ReplaceVariables(2);

        var expected = "";

        (found == expected)
            .Assert
            (() =>
                "\nXpctd: " + expected + "\nFound: " + found + "\nText : " + text
            );
    }
}