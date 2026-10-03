using System.Text.Json.Serialization;

namespace Stoat.Api.Model;

public class StoatStatus
{
    [JsonPropertyName("revolt")] public string? Revolt { get; set; }
    [JsonPropertyName("ws")] public string? Ws { get; set; }
    [JsonPropertyName("app")] public string? App { get; set; }
    [JsonPropertyName("vapid")] public string? Vapid { get; set; }
    
    [JsonPropertyName("features")] public StoatFeatures? Features { get; set; }
}

public class StoatFeatures
{
    [JsonPropertyName("captcha")] public StoatInfo? Captcha { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("invite_only")] public bool? InviteOnly { get; set; }
    [JsonPropertyName("autumn")] public StoatInfo? Autumn { get; set; }
    [JsonPropertyName("january")] public StoatInfo? January { get; set; }
    [JsonPropertyName("livekit")] public StoatInfo? LiveKit { get; set; }
    [JsonPropertyName("limits")] public StoatLimits? Limits { get; set; }
    
}

public class StoatInfo
{
    [JsonPropertyName("enabled")] public bool? Enabled { get; set; }
        
    [JsonPropertyName("enabled")] public string? Key { get; set; }
    
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }
        
    [JsonPropertyName("nodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<StoatNode>? Nodes { get; set; }
        
        
    public class StoatNode
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
            
        [JsonPropertyName("lat")]
        public double? Lat { get; set; }
            
        [JsonPropertyName("lon")]
        public double? Lon { get; set; }
            
        [JsonPropertyName("public_url")]
        public string? PublicUrl { get; set; }
    }
}

public class StoatLimits
{
    [JsonPropertyName("global")]
    public StoatGlobal? Global { get; set; }
    
    [JsonPropertyName("new_user")]
    public StoatOptions? NewUser { get; set; }
    
    [JsonPropertyName("default")] 
    public StoatOptions? Default { get; set; }
    
    public class StoatGlobal
    {
        [JsonPropertyName("group_size")] public int GroupSize { get; set; }
        [JsonPropertyName("message_embeds")] public int MessageEmbeds { get; set; }
        [JsonPropertyName("message_replies")] public int MessageReplies { get; set; }
        [JsonPropertyName("message_reactions")] public int MessageReactions { get; set; }
        [JsonPropertyName("server_emoji")] public int ServerEmoji { get; set; }
        [JsonPropertyName("server_roles")] public int ServerRoles { get; set; }
        [JsonPropertyName("server_channels")] public int ServerChannels { get; set; }
        [JsonPropertyName("body_limit_size")] public int BodyLimitSize { get; set; }
        [JsonPropertyName("restrict_server_creation")] public ICollection<string>? RestrictServerCreation { get; set; }
        [JsonPropertyName("new_user_hours")] public int NewUserHours { get; set; }
    }
    
    public class StoatOptions
    {
        [JsonPropertyName("outgoing_friend_requests")] public int OutgoingFriendRequests { get; set; }
        [JsonPropertyName("bots")] public int Bots { get; set; }
        [JsonPropertyName("message_length")] public int MessageLength { get; set; }
        [JsonPropertyName("message_attachments")] public int MessageAttachments { get; set; }
        [JsonPropertyName("servers")] public int Servers { get; set; }
        [JsonPropertyName("voice_quality")] public int VoiceQuality { get; set; }
        [JsonPropertyName("video")] public bool Video { get; set; }
        [JsonPropertyName("video_resolution")] public ICollection<int>? VideoResolution { get; set; }
        [JsonPropertyName("video_aspect_ratio")] public ICollection<int>? VideoAspectRatio { get; set; }
        [JsonPropertyName("file_upload_size_limits")] public Dictionary<string, int>? FileUploadSizeLimits { get; set; }
    }
}

public class StoatLegalLinks
{
    [JsonPropertyName("terms_of_service")] public string? Terms { get; set; }
    [JsonPropertyName("privacy_policy")] public string? PrivacyPolicy { get; set; }
    [JsonPropertyName("guidelines")] public string? Guidelines { get; set; }
}
