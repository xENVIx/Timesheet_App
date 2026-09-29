using System.ComponentModel;
using Timesheeter.Lib;

namespace Timesheeter.Tests;

public class BindingListViewTests
{
    public class Entry
    {
        public long ID { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly TimeStart { get; set; }
        public long ProjectCodeID { get; set; }
        public string? Note { get; set; }
        public bool Billable { get; set; }
    }

    private static Entry E(long id, int day, int hour, long code = 1, string? note = null) =>
        new() { ID = id, Date = new DateOnly(2026, 9, day), TimeStart = new TimeOnly(hour, 0), ProjectCodeID = code, Note = note };

    private static BindingListView<Entry> ListOf(params Entry[] entries)
    {
        var list = new BindingListView<Entry>();
        foreach (var e in entries) list.Add(e);
        return list;
    }

    private static long[] Ids(IEnumerable<Entry> entries) => entries.Select(e => e.ID).ToArray();

    // ---- Sorting ----

    [Fact]
    public void Sort_MultipleProperties()
    {
        var list = ListOf(E(1, 30, 9), E(2, 29, 14), E(3, 29, 8), E(4, 30, 7));

        list.Sort = "Date, TimeStart";

        Assert.Equal([3, 2, 4, 1], Ids(list));
        Assert.Equal("Date, TimeStart", list.Sort);
    }

    [Fact]
    public void Sort_Descending()
    {
        var list = ListOf(E(1, 1, 9), E(2, 3, 9), E(3, 2, 9));

        list.Sort = "Date DESC";

        Assert.Equal([2, 3, 1], Ids(list));
    }

    [Fact]
    public void Sort_IsStableForEqualValues()
    {
        var list = ListOf(E(1, 5, 9), E(2, 5, 9), E(3, 4, 9), E(4, 5, 9));

        list.Sort = "Date";

        Assert.Equal([3, 1, 2, 4], Ids(list));
    }

    [Fact]
    public void Sort_ViaIBindingListView_LikeBindingSourceDoes()
    {
        var list = ListOf(E(1, 30, 9), E(2, 29, 14), E(3, 29, 8));
        var props = TypeDescriptor.GetProperties(typeof(Entry));
        IBindingListView view = list;

        view.ApplySort(new ListSortDescriptionCollection(
        [
            new ListSortDescription(props["Date"]!, ListSortDirection.Ascending),
            new ListSortDescription(props["TimeStart"]!, ListSortDirection.Ascending),
        ]));

        Assert.True(view.SupportsAdvancedSorting);
        Assert.Equal([3, 2, 1], Ids(list));
    }

    [Fact]
    public void Sort_SingleColumn_LikeGridHeaderClick()
    {
        var list = ListOf(E(1, 1, 9, code: 3), E(2, 1, 9, code: 1), E(3, 1, 9, code: 2));
        IBindingList bindingList = list;

        bindingList.ApplySort(TypeDescriptor.GetProperties(typeof(Entry))["ProjectCodeID"]!, ListSortDirection.Descending);

        Assert.True(bindingList.IsSorted);
        Assert.Equal("ProjectCodeID", bindingList.SortProperty!.Name);
        Assert.Equal(ListSortDirection.Descending, bindingList.SortDirection);
        Assert.Equal([1, 3, 2], Ids(list));
    }

    [Fact]
    public void Sort_NullsFirst_TextIgnoresCase()
    {
        var list = ListOf(E(1, 1, 9, note: "banana"), E(2, 1, 9, note: null), E(3, 1, 9, note: "Apple"));

        list.Sort = "Note";

        Assert.Equal([2, 3, 1], Ids(list));
    }

    [Fact]
    public void RemoveSort_RestoresAddedOrder()
    {
        var list = ListOf(E(1, 3, 9), E(2, 1, 9), E(3, 2, 9));
        list.Sort = "Date";

        list.Sort = null;

        Assert.False(((IBindingList)list).IsSorted);
        Assert.Equal([1, 2, 3], Ids(list));
    }

    [Fact]
    public void Add_WhileSorted_InsertsInSortedPosition()
    {
        var list = ListOf(E(1, 1, 9), E(2, 3, 9));
        list.Sort = "Date";

        list.Add(E(3, 2, 9));

        Assert.Equal([1, 3, 2], Ids(list));
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("Date SIDEWAYS")]
    public void Sort_Invalid_Throws(string sort)
    {
        var list = ListOf(E(1, 1, 9));
        Assert.Throws<ArgumentException>(() => list.Sort = sort);
    }

    // ---- Filtering ----

    [Fact]
    public void Filter_DateRangeAndIn()
    {
        var list = ListOf(E(1, 1, 9, code: 3), E(2, 15, 9, code: 5), E(3, 15, 9, code: 7), E(4, 30, 9, code: 3));

        list.Filter = "Date >= '2026-09-10' AND Date <= '2026-09-30' AND ProjectCodeID IN (3, 5)";

        Assert.Equal([2, 4], Ids(list));
        Assert.Equal(4, list.Unfiltered.Count);
    }

    [Theory]
    [InlineData("ProjectCodeID = 2", new long[] { 2 })]
    [InlineData("ProjectCodeID <> 2", new long[] { 1, 3 })]
    [InlineData("ProjectCodeID != 2", new long[] { 1, 3 })]
    [InlineData("ProjectCodeID < 2", new long[] { 1 })]
    [InlineData("ProjectCodeID <= 2", new long[] { 1, 2 })]
    [InlineData("ProjectCodeID > 2", new long[] { 3 })]
    [InlineData("ProjectCodeID >= 2", new long[] { 2, 3 })]
    [InlineData("projectcodeid in (1,3)", new long[] { 1, 3 })]
    [InlineData("[ProjectCodeID]=3", new long[] { 3 })]
    public void Filter_Operators(string filter, long[] expected)
    {
        var list = ListOf(E(1, 1, 9, code: 1), E(2, 1, 9, code: 2), E(3, 1, 9, code: 3));

        list.Filter = filter;

        Assert.Equal(expected, Ids(list));
    }

    [Fact]
    public void Filter_TimeOnlyAndBool()
    {
        var list = ListOf(E(1, 1, 8), E(2, 1, 13), E(3, 1, 17));
        list[1].Billable = true;
        list[2].Billable = true;

        list.Filter = "TimeStart >= '12:00' AND Billable = true";

        Assert.Equal([2, 3], Ids(list));
    }

    [Fact]
    public void Filter_TextIgnoresCaseAndSupportsEscapedQuotes()
    {
        var list = ListOf(E(1, 1, 9, note: "Bob's job"), E(2, 1, 9, note: "other"), E(3, 1, 9, note: null));

        list.Filter = "Note = 'BOB''S JOB'";
        Assert.Equal([1], Ids(list));

        list.Filter = "Note <> 'other'";
        Assert.Equal([1, 3], Ids(list));
    }

    [Theory]
    [InlineData("Missing = 1")]
    [InlineData("ProjectCodeID ~ 1")]
    [InlineData("ProjectCodeID = abc")]
    [InlineData("Date = 'not a date'")]
    [InlineData("Note = 'unterminated")]
    [InlineData("ProjectCodeID IN (1, 2")]
    [InlineData("ProjectCodeID = 1 OR ProjectCodeID = 2")]
    public void Filter_Invalid_ThrowsAndKeepsCurrentFilter(string filter)
    {
        var list = ListOf(E(1, 1, 9, code: 1), E(2, 1, 9, code: 2));
        list.Filter = "ProjectCodeID = 1";

        Assert.Throws<ArgumentException>(() => list.Filter = filter);

        Assert.Equal("ProjectCodeID = 1", list.Filter);
        Assert.Equal([1], Ids(list));
    }

    [Fact]
    public void RemoveFilter_ShowsEverythingAgain_KeepsSort()
    {
        var list = ListOf(E(1, 3, 9, code: 1), E(2, 1, 9, code: 2), E(3, 2, 9, code: 1));
        list.Sort = "Date";
        list.Filter = "ProjectCodeID = 1";
        Assert.Equal([3, 1], Ids(list));

        list.RemoveFilter();

        Assert.Null(list.Filter);
        Assert.Equal([2, 3, 1], Ids(list));
    }

    [Fact]
    public void Add_NotMatchingFilter_IsKeptButHidden()
    {
        var list = ListOf(E(1, 1, 9, code: 1));
        list.Filter = "ProjectCodeID = 1";

        list.Add(E(2, 1, 9, code: 2));

        Assert.Equal([1], Ids(list));
        Assert.Equal([1, 2], Ids(list.Unfiltered));

        list.RemoveFilter();
        Assert.Equal([1, 2], Ids(list));
    }

    // ---- Keeping visible and unfiltered items in step ----

    [Fact]
    public void Remove_RemovesFromUnfilteredToo()
    {
        var list = ListOf(E(1, 1, 9), E(2, 1, 9));

        list.RemoveAt(0);

        Assert.Equal([2], Ids(list.Unfiltered));
    }

    [Fact]
    public void Clear_ClearsUnfilteredToo()
    {
        var list = ListOf(E(1, 1, 9), E(2, 1, 9));
        list.Filter = "ID = 1";

        list.Clear();

        Assert.Empty(list);
        Assert.Empty(list.Unfiltered);
    }

    [Fact]
    public void Replace_UpdatesUnfilteredAndPosition()
    {
        var list = ListOf(E(1, 1, 9), E(2, 2, 9), E(3, 3, 9));
        list.Sort = "Date";

        list[0] = E(4, 5, 9); // e.g. DataClass.Reload swapping in the database copy

        Assert.Equal([2, 3, 4], Ids(list));
        Assert.Equal([4, 2, 3], Ids(list.Unfiltered));
    }

    [Fact]
    public void Reposition_AfterEdit_MovesItemOrHidesIt()
    {
        var list = ListOf(E(1, 1, 9), E(2, 2, 9), E(3, 3, 9));
        list.Sort = "Date";
        list.Filter = "Date <= '2026-09-10'";

        var first = list[0];
        first.Date = new DateOnly(2026, 9, 4);
        list.Reposition(first);
        Assert.Equal([2, 3, 1], Ids(list));

        first.Date = new DateOnly(2026, 9, 20);
        list.Reposition(first);
        Assert.Equal([2, 3], Ids(list));

        first.Date = new DateOnly(2026, 9, 1);
        list.Reposition(first);
        Assert.Equal([1, 2, 3], Ids(list));
    }

    [Fact]
    public void Filter_And_Sort_RaiseASingleReset()
    {
        var list = ListOf(E(1, 2, 9), E(2, 1, 9));
        var events = new List<ListChangedType>();
        list.ListChanged += (_, e) => events.Add(e.ListChangedType);

        list.Sort = "Date";
        list.Filter = "ID = 1";

        Assert.Equal([ListChangedType.Reset, ListChangedType.Reset], events);
    }
}
