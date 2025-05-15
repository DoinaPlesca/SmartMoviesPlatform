namespace SharedKernel.Wrappers;

public class PagedResponse<T> : ApiResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public static PagedResponse<T> Ok(T data, int totalCount, int page, int pageSize)
    {
        return new PagedResponse<T>
        {
            Success = true,
            Data = data,
            Message = null,
            Status = 200,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
