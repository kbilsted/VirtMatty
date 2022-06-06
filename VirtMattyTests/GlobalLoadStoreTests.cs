public class GlobalLoadStoreTests
{
    [Test]
    public void When_loading_from_heap_Then_load_stored_value()
    {
        var output = AsmRunner.Go(@"
    pushi   1
    store   44
    load    44
    print
    halt
");
        Assert.True(output.Single().ToString() == "1");
    }

    [Test]
    public void When_swap_order_on_print_by_storing_in_heap_Then_load_stored_value()
    {
        var output = AsmRunner.Go(@"
    pushi   1
    pushi   2
    store 44  ; 2
    store 55  ; 1
    load 44   ; 2
    load 55   ; 1
    print
    print
    halt
");

        CollectionAssert.AreEquivalent(new[] { 1, 2 }, output);
    }

}
