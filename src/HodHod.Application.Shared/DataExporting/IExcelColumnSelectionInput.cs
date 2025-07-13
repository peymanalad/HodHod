using System.Collections.Generic;

namespace HodHod.DataExporting;

public interface IExcelColumnSelectionInput
{
    List<string> SelectedColumns { get; set; }
}

