using OneOf;

namespace Eluvion.Effect;

/// <summary>An effect that fires the matching handler for a 2-case union.</summary>
public sealed class Cases<T0, T1>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1
) : EffectEnvelope<OneOf<T0, T1>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 3-case union.</summary>
public sealed class Cases<T0, T1, T2>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2
) : EffectEnvelope<OneOf<T0, T1, T2>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 4-case union.</summary>
public sealed class Cases<T0, T1, T2, T3>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3
) : EffectEnvelope<OneOf<T0, T1, T2, T3>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 5-case union.</summary>
public sealed class Cases<T0, T1, T2, T3, T4>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3,
    Func<T4, IEffect<T4>> on4
) : EffectEnvelope<OneOf<T0, T1, T2, T3, T4>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3),
        t4 => on4(t4).Fire(t4)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 6-case union.</summary>
public sealed class Cases<T0, T1, T2, T3, T4, T5>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3,
    Func<T4, IEffect<T4>> on4,
    Func<T5, IEffect<T5>> on5
) : EffectEnvelope<OneOf<T0, T1, T2, T3, T4, T5>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3),
        t4 => on4(t4).Fire(t4),
        t5 => on5(t5).Fire(t5)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 7-case union.</summary>
public sealed class Cases<T0, T1, T2, T3, T4, T5, T6>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3,
    Func<T4, IEffect<T4>> on4,
    Func<T5, IEffect<T5>> on5,
    Func<T6, IEffect<T6>> on6
) : EffectEnvelope<OneOf<T0, T1, T2, T3, T4, T5, T6>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3),
        t4 => on4(t4).Fire(t4),
        t5 => on5(t5).Fire(t5),
        t6 => on6(t6).Fire(t6)
    )
)
{ }

/// <summary>An effect that fires the matching handler for an 8-case union.</summary>
public sealed class Cases<T0, T1, T2, T3, T4, T5, T6, T7>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3,
    Func<T4, IEffect<T4>> on4,
    Func<T5, IEffect<T5>> on5,
    Func<T6, IEffect<T6>> on6,
    Func<T7, IEffect<T7>> on7
) : EffectEnvelope<OneOf<T0, T1, T2, T3, T4, T5, T6, T7>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3),
        t4 => on4(t4).Fire(t4),
        t5 => on5(t5).Fire(t5),
        t6 => on6(t6).Fire(t6),
        t7 => on7(t7).Fire(t7)
    )
)
{ }

/// <summary>An effect that fires the matching handler for a 9-case union.</summary>
public sealed class Cases<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
    Func<T0, IEffect<T0>> on0,
    Func<T1, IEffect<T1>> on1,
    Func<T2, IEffect<T2>> on2,
    Func<T3, IEffect<T3>> on3,
    Func<T4, IEffect<T4>> on4,
    Func<T5, IEffect<T5>> on5,
    Func<T6, IEffect<T6>> on6,
    Func<T7, IEffect<T7>> on7,
    Func<T8, IEffect<T8>> on8
) : EffectEnvelope<OneOf<T0, T1, T2, T3, T4, T5, T6, T7, T8>>(
    oneOf => oneOf.Match(
        t0 => on0(t0).Fire(t0),
        t1 => on1(t1).Fire(t1),
        t2 => on2(t2).Fire(t2),
        t3 => on3(t3).Fire(t3),
        t4 => on4(t4).Fire(t4),
        t5 => on5(t5).Fire(t5),
        t6 => on6(t6).Fire(t6),
        t7 => on7(t7).Fire(t7),
        t8 => on8(t8).Fire(t8)
    )
)
{ }
