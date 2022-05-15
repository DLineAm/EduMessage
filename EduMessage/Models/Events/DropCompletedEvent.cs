using System.Collections.Generic;
using Windows.Storage;

namespace EduMessage.Pages
{
    public record DropCompletedEvent(IReadOnlyList<IStorageItem> Items);
}