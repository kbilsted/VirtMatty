public class JmpTests
{
    [Test]
    public void When_jumping_to_label_jumps_over_print2_Then_only_print_1()
    {
        var output = AsmRunner.Go(@"
    pushi   1
    print
    jmp     skip_print_2
    pushi   2
    print
skip_print_2:
    halt
");

        Assert.True(output.Single().ToString() == "1");
    }


    [Test]
    public void When_true_and_instr_branchtrue_Then_print_1()
    {
        var output = AsmRunner.Go(@"
    pushi   2
    pushi   1
    brt print
    halt
print:
    print
    halt
");

        Assert.AreEqual(2, output.Single());
    }

    [Test]
    public void When_false_and_instr_branchtrue_Then_dont()
    {
        var output = AsmRunner.Go(@"
    pushi   2
    pushi   0
    brt print
    halt
print:
    print
    halt
");

        CollectionAssert.IsEmpty(output);
    }


    [Test]
    public void When_false_and_instr_branchfalse_Then_print_1()
    {
        var output = AsmRunner.Go(@"
    pushi   2
    pushi   0
    brf print
    halt
print:
    print
    halt
");

        Assert.AreEqual(2, output.Single());
    }

    [Test]
    public void When_true_and_instr_branchfalse_Then_dont()
    {
        var output = AsmRunner.Go(@"
    pushi   2
    pushi   1
    brf print
    halt
print:
    print
    halt
");

        CollectionAssert.IsEmpty(output);
    }

    [Test]
    public void When_jumping_to_an_illegal_label_Then_fail()
    {
        Assert.Throws<KeyNotFoundException>(() => AsmRunner.Go(@"jmp skip_print_2"));
    }
}
