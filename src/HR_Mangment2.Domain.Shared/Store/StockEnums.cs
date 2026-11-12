namespace HR_Mangment2.Store;

public enum ItemKind
{
    Returnable,
    Consumable
}

public enum StockIssueStatus
{
    PendingAcknowledgement,
    Acknowledged,
    Purchasing,
    Delivered,
    Cancelled
}

public enum StockMovementType
{
    Receive,
    Issue,
    Return,
    Adjustment,

    /// <summary>Stock in from an approved purchase receipt (decision 2026-11-11).</summary>
    Purchase
}

public enum CustodyKind
{
    Personal,
    Shared
}

public enum CustodyStatus
{
    PendingAcknowledgement,
    Acknowledged,
    Returned
}

public enum CustodyCountResult
{
    Matched,
    Discrepancy
}

/// <summary>Derived from an item's balance and minimum, never stored.</summary>
public enum StockStatus
{
    InStock,
    LowStock,
    OutOfStock
}

/// <summary>A purchase receipt or an issue voucher. Only an approved one has touched the balance.</summary>
public enum StoreDocumentStatus
{
    Draft,
    Approved,
    Reversed
}

public enum StoreReportCode
{
    Purchases,
    CurrentStock,
    LowStock,
    OutOfStock,
    IssuesByTeam,
    StockValue,
    ItemMovement
}
