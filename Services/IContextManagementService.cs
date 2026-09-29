using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;

namespace BackendEgitimiYeni.Services;

public interface IContextManagementService
{
    List<RagSourceDto> LimitSources(
        List<RagSourceDto> sources
    );

    List<ChatMessage> LimitHistory(
        List<ChatMessage> history
    );
}