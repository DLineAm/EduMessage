using EduMessage.ViewModels;
using Mapster;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public class MapsterConfig : ICodeGenerationRegister
    {
        public void Register(CodeGenerationConfig config)
        {
            config.AdaptTo(nameof(FormattedMessage))
                .ForType<MessageAttachment>(builder =>
                {
                    builder.Map(p => p.IdMessageNavigation, nameof(FormattedMessage.Message));
                    builder.Map(p => p.IdAttachmentNavigation, nameof(FormattedMessage.Attachments));
                });
        }
    }
}