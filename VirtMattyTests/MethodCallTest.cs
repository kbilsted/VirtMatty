public class MethodCallTest
{
    [Test]
    public void When_calling_a_method_Then_it_is_called()
    {
        var output = AsmRunner.Go(@"
    pushi 1
    print
    call f
    pushi 3
    print
    halt", ("f", new string[0], @"pushi 2
                                 print
                                 ret"));

        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, output);
    }


    [Test]
    public void When_calling_fibonaci_0_1__1_Then_it_calculates_0()
    {
        var output = AsmRunner.Go(@"
    pushi 0
    pushi 1
    pushi 1
    call fibo
    halt",
    ("fibo", new string[] { "a", "b", "left" }, @"
        pusharg left
        pushi 1
        cmp
        jumpless done

        pusharg a
        print

        pusharg b

        pusharg a
        pusharg b
        add

        pusharg left
        pushi 1
        sub

        call fibo
done:
        ret"));

        CollectionAssert.AreEquivalent(new[] { 0 }, output);
    }

    [Test]
    [Category("con")]
    public void When_calling_fibonaci_0_1_6_Then_it_calculates_correctly()
    {
        var output = AsmRunner.Go(@"
    pushi 0
    pushi 1
    pushi 9
    call fibo
    halt",
    ("fibo", new string[] { "a", "b", "left" }, @"
        pusharg left
        pushi 1
        cmp
        jumpless done

        pusharg a
        print

        pusharg b

        pusharg a
        pusharg b
        add

        pusharg left
        pushi 1
        sub

        call fibo
done:
        ret"));

        CollectionAssert.AreEquivalent(new[] { 0, 1, 1, 2, 3, 5, 8, 13, 21 }, output);
    }


}
