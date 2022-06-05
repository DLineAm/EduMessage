using System;

namespace EduMessage.Services
{
    public interface IFactory
    {
        object Realise(Type elementType);
    }
}