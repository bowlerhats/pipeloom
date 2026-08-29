using System;

namespace PipeLoom.Engine.Abstractions;

public enum HandlerRole
{
    None = 0,
    Mapper = 1,
    Transformer = 2,
    Reducer = 3,
    Expander = 4,
    Bundler = 5
}

// public static class HandlerRoleUtils
// {
//     internal static bool IsLowerOrEquallyRanked(HandlerRole first, HandlerRole thanSecond)
//     {
//         if (first == HandlerRole.None || thanSecond == HandlerRole.None)
//             return false;
//         
//         if (first == thanSecond || thanSecond == HandlerRole.Bundler)
//             return true;
//         
//         
//         throw new NotImplementedException();
//     }
// }