// namespace PipeLoom.Types.Abstractions;
//
// public interface IPlScalar<TNative> : IPlValue<TNative>
// {
//     PlScalar<TNative> ToScalar();
// }
//
// public readonly record struct PlScalar<TNative>(
//     TNative Value,
//     IPlScalarDef<TNative>? TypeDef
// ) : IPlScalar<TNative>
// {
//     public PlScalar<TNative> ToScalar()
//     {
//         return this;
//     }
//
//     public PlValue<TNative, IPlScalarDef<TNative>> ToValue()
//     {
//         return new PlValue<TNative, IPlScalarDef<TNative>>(this.Value, this.TypeDef);
//     }
//
//     public static implicit operator TNative(scoped in PlScalar<TNative> v)
//     {
//         return v.Value;
//     }
// }