using BeautyBeast.Web.Models;

namespace BeautyBeast.Web.Services;

public class ProfileService
{
    private readonly ApiClient _api;
    public ProfileService(ApiClient api) => _api = api;

    public Task<List<UserSummary>?> ListArtistsAsync(string? q = null) =>
        _api.GetAsync<List<UserSummary>>($"/api/artists{(string.IsNullOrWhiteSpace(q) ? "" : $"?q={Uri.EscapeDataString(q)}")}");

    public Task<ArtistProfileDto?> GetArtistAsync(string username) =>
        _api.GetAsync<ArtistProfileDto>($"/api/artists/{Uri.EscapeDataString(username)}");

    public Task<ClientProfileDto?> GetClientAsync(string username) =>
        _api.GetAsync<ClientProfileDto>($"/api/clients/{Uri.EscapeDataString(username)}");

    public Task UpdateArtistAsync(UpdateArtistProfileRequest req) =>
        _api.PutAsync("/api/artists/me", req);

    public Task UpdateClientAsync(UpdateClientProfileRequest req) =>
        _api.PutAsync("/api/clients/me", req);

    public Task<List<Achievement>?> ListAchievementsAsync(int userId) =>
        _api.GetAsync<List<Achievement>>($"/api/users/{userId}/achievements");

    public Task<Achievement?> AddAchievementAsync(CreateAchievementRequest req) =>
        _api.PostAsync<Achievement>("/api/achievements", req);

    public Task DeleteAchievementAsync(int id) =>
        _api.DeleteAsync($"/api/achievements/{id}");
}

public class PostService
{
    private readonly ApiClient _api;
    public PostService(ApiClient api) => _api = api;

    public Task<List<Post>?> FeedAsync(int? authorId = null, string? author = null, int? take = null)
    {
        var qs = new List<string>();
        if (authorId.HasValue) qs.Add($"authorId={authorId.Value}");
        if (!string.IsNullOrWhiteSpace(author)) qs.Add($"author={Uri.EscapeDataString(author)}");
        if (take.HasValue) qs.Add($"take={take.Value}");
        var query = qs.Count == 0 ? "" : "?" + string.Join("&", qs);
        return _api.GetAsync<List<Post>>($"/api/posts/{query}");
    }

    public Task<Post?> GetAsync(int id) => _api.GetAsync<Post>($"/api/posts/{id}");
    public Task<Post?> CreateAsync(CreatePostRequest req) => _api.PostAsync<Post>("/api/posts/", req);
    public Task UpdateAsync(int id, CreatePostRequest req) => _api.PutAsync($"/api/posts/{id}", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/posts/{id}");

    public Task LikeAsync(int id) => _api.PostAsync($"/api/posts/{id}/like", null);
    public Task UnlikeAsync(int id) => _api.DeleteAsync($"/api/posts/{id}/like");

    public Task<List<Comment>?> CommentsAsync(int postId) => _api.GetAsync<List<Comment>>($"/api/posts/{postId}/comments");
    public Task<Comment?> AddCommentAsync(int postId, CreateCommentRequest req) => _api.PostAsync<Comment>($"/api/posts/{postId}/comments", req);
    public Task DeleteCommentAsync(int commentId) => _api.DeleteAsync($"/api/posts/comments/{commentId}");
}

public class StatusService
{
    private readonly ApiClient _api;
    public StatusService(ApiClient api) => _api = api;

    public Task<List<StatusItem>?> ListAsync(string? author = null, int? take = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(author)) qs.Add($"author={Uri.EscapeDataString(author)}");
        if (take.HasValue) qs.Add($"take={take.Value}");
        var query = qs.Count == 0 ? "" : "?" + string.Join("&", qs);
        return _api.GetAsync<List<StatusItem>>($"/api/statuses/{query}");
    }

    public Task<StatusItem?> CreateAsync(CreateStatusRequest req) => _api.PostAsync<StatusItem>("/api/statuses/", req);
    public Task UpdateAsync(int id, CreateStatusRequest req) => _api.PutAsync($"/api/statuses/{id}", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/statuses/{id}");

    public Task LikeAsync(int id) => _api.PostAsync($"/api/statuses/{id}/like", null);
    public Task UnlikeAsync(int id) => _api.DeleteAsync($"/api/statuses/{id}/like");

    public Task<List<StatusComment>?> CommentsAsync(int statusId) =>
        _api.GetAsync<List<StatusComment>>($"/api/statuses/{statusId}/comments");

    public Task<StatusComment?> AddCommentAsync(int statusId, CreateStatusCommentRequest req) =>
        _api.PostAsync<StatusComment>($"/api/statuses/{statusId}/comments", new CreateCommentRequest(req.Text));

    public Task DeleteCommentAsync(int commentId) =>
        _api.DeleteAsync($"/api/statuses/comments/{commentId}");
}

public class NotificationService
{
    private readonly ApiClient _api;
    public NotificationService(ApiClient api) => _api = api;

    public Task<List<Notification>?> ListAsync() =>
        _api.GetAsync<List<Notification>>("/api/notifications");

    public async Task<int> UnreadCountAsync()
    {
        var result = await _api.GetAsync<UnreadCountResponse>("/api/notifications/unread-count");
        return result?.Count ?? 0;
    }

    public Task MarkAllReadAsync() => _api.PutAsync("/api/notifications/read-all", null);
    public Task MarkReadAsync(int id) => _api.PutAsync($"/api/notifications/{id}/read", null);
}

// Internal helper record for unread-count endpoint
file record UnreadCountResponse(int Count);

public class TreatmentService
{
    private readonly ApiClient _api;
    public TreatmentService(ApiClient api) => _api = api;

    public Task<List<Treatment>?> ListAsync(int? artistId = null, string? q = null, string? category = null)
    {
        var qs = new List<string>();
        if (artistId.HasValue) qs.Add($"artistId={artistId.Value}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");
        if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        var query = qs.Count == 0 ? "" : "?" + string.Join("&", qs);
        return _api.GetAsync<List<Treatment>>($"/api/treatments/{query}");
    }

    public Task<Treatment?> GetAsync(int id) => _api.GetAsync<Treatment>($"/api/treatments/{id}");
    public Task<Treatment?> CreateAsync(CreateTreatmentRequest req) => _api.PostAsync<Treatment>("/api/treatments/", req);
    public Task UpdateAsync(int id, CreateTreatmentRequest req) => _api.PutAsync($"/api/treatments/{id}", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/treatments/{id}");

    public Task<List<string>?> CategoriesAsync() => _api.GetAsync<List<string>>("/api/treatments/categories");
}

public class BookingService
{
    private readonly ApiClient _api;
    public BookingService(ApiClient api) => _api = api;

    public Task<List<Booking>?> MineAsync() => _api.GetAsync<List<Booking>>("/api/bookings/me");
    public Task<Booking?> GetAsync(int id) => _api.GetAsync<Booking>($"/api/bookings/{id}");
    public Task<Booking?> CreateAsync(CreateBookingRequest req) => _api.PostAsync<Booking>("/api/bookings/", req);
    public Task SetStatusAsync(int id, UpdateBookingStatusRequest req) => _api.PutAsync($"/api/bookings/{id}/status", req);
}

public class ReviewService
{
    private readonly ApiClient _api;
    public ReviewService(ApiClient api) => _api = api;

    public Task<List<Review>?> ForArtistAsync(int artistId) => _api.GetAsync<List<Review>>($"/api/reviews/?artistId={artistId}");
    public Task<Review?> CreateAsync(CreateReviewRequest req) => _api.PostAsync<Review>("/api/reviews/", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/reviews/{id}");
}

public class ResourceService
{
    private readonly ApiClient _api;
    public ResourceService(ApiClient api) => _api = api;

    public Task<List<Resource>?> ListAsync(string? q = null, string? category = null, int? authorId = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q)}");
        if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        if (authorId.HasValue) qs.Add($"authorId={authorId.Value}");
        var query = qs.Count == 0 ? "" : "?" + string.Join("&", qs);
        return _api.GetAsync<List<Resource>>($"/api/resources/{query}");
    }

    public Task<Resource?> GetAsync(int id) => _api.GetAsync<Resource>($"/api/resources/{id}");
    public Task<Resource?> CreateAsync(CreateResourceRequest req) => _api.PostAsync<Resource>("/api/resources/", req);
    public Task UpdateAsync(int id, CreateResourceRequest req) => _api.PutAsync($"/api/resources/{id}", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/resources/{id}");
}

public class ConsentService
{
    private readonly ApiClient _api;
    public ConsentService(ApiClient api) => _api = api;

    public Task<List<ConsentForm>?> ListAsync(string? category = null)
    {
        var query = string.IsNullOrWhiteSpace(category) ? "" : $"?category={Uri.EscapeDataString(category)}";
        return _api.GetAsync<List<ConsentForm>>($"/api/consent-forms/{query}");
    }

    public Task<ConsentForm?> GetAsync(int id) => _api.GetAsync<ConsentForm>($"/api/consent-forms/{id}");
    public Task<ConsentForm?> CreateAsync(CreateConsentFormRequest req) => _api.PostAsync<ConsentForm>("/api/consent-forms/", req);
    public Task UpdateAsync(int id, CreateConsentFormRequest req) => _api.PutAsync($"/api/consent-forms/{id}", req);
    public Task DeleteAsync(int id) => _api.DeleteAsync($"/api/consent-forms/{id}");
}

public class SearchService
{
    private readonly ApiClient _api;
    public SearchService(ApiClient api) => _api = api;
    public Task<SearchResults?> SearchAsync(string q) => _api.GetAsync<SearchResults>($"/api/search?q={Uri.EscapeDataString(q)}");
}

public class ScheduleService
{
    private readonly ApiClient _api;
    public ScheduleService(ApiClient api) => _api = api;

    public Task<List<WorkingDayDto>?> GetMineAsync() =>
        _api.GetAsync<List<WorkingDayDto>>("/api/me/schedule");

    public Task<List<WorkingDayDto>?> GetForArtistAsync(int artistId) =>
        _api.GetAsync<List<WorkingDayDto>>($"/api/artists/{artistId}/schedule");

    public Task SetMineAsync(SetScheduleRequest req) =>
        _api.PutAsync("/api/me/schedule", req);

    public Task<SlotsResponse?> GetSlotsAsync(int treatmentId, DateTime localDate) =>
        _api.GetAsync<SlotsResponse>($"/api/treatments/{treatmentId}/slots?date={localDate:yyyy-MM-dd}");
}

public class MessagesService
{
    private readonly ApiClient _api;
    public MessagesService(ApiClient api) => _api = api;

    public Task<List<ConversationItem>?> ListConversationsAsync() =>
        _api.GetAsync<List<ConversationItem>>("/api/conversations");

    public Task<ConversationItem?> GetOrCreateAsync(int otherUserId) =>
        _api.PostAsync<ConversationItem>($"/api/conversations/with/{otherUserId}", null);

    public Task<List<DirectMessageItem>?> GetMessagesAsync(int conversationId, int? before = null, int take = 50)
    {
        var qs = $"take={take}";
        if (before.HasValue) qs += $"&before={before.Value}";
        return _api.GetAsync<List<DirectMessageItem>>($"/api/conversations/{conversationId}/messages?{qs}");
    }

    public Task<DirectMessageItem?> SendAsync(int conversationId, SendMessageRequest req) =>
        _api.PostAsync<DirectMessageItem>($"/api/conversations/{conversationId}/messages", req);

    public Task MarkReadAsync(int conversationId) =>
        _api.PutAsync($"/api/conversations/{conversationId}/read", null);
}

public class UploadService
{
    private readonly ApiClient _api;
    public UploadService(ApiClient api) => _api = api;

    public async Task<string?> UploadAsync(Stream stream, string filename, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", filename);
        var resp = await _api.PostMultipartAsync<UploadResponse>("/api/uploads/", content, ct);
        return resp?.Url;
    }
}
