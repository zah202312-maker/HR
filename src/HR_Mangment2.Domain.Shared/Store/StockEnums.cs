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
    Adjustment
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
