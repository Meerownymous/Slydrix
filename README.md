[![EO principles respected here](http://www.elegantobjects.org/badge.svg)](http://www.elegantobjects.org)

# Eluvion

Use cases as objects, for .NET. A flow is built from four primitives, each of which is a class you can name, hold, test and replace.

Eluvion builds on [Tonga](https://github.com/Meerownymous/Tonga) and follows the same rules — [Elegant Objects](http://www.elegantobjects.org), both volumes. Where Tonga gives you objects for values, sequences and streams, Eluvion gives you objects for the steps of a use case.

Target: `net9.0`. Depends on [Tonga](https://www.nuget.org/packages/Tonga) and [OneOf](https://github.com/mcintyre321/OneOf). No version has been tagged yet, so there is no package on NuGet.

## Why Eluvion

```csharp
var post =
    await new SeedFromJson<CreatePostCommand>(body)
        .Craft(new WithAuthor(userId, userRepo))
        .Craft(new AsPost())
        .Effect(new InRepo<Post>(postRepo))
        .Effect(new InSearchIndex(searchIndex))
        .Trigger(new Published<PostCreated>(eventBus))
        .Yield();
```

Read top to bottom: what comes in, what changes it, what is stored, what is announced, what comes out. Nothing is hidden between the lines — there is no branch, no early return, no framework deciding the order.

What differs is underneath. Every step is an object:

```csharp
var withAuthor = new WithAuthor(userId, userRepo);   // an ICraft<CreatePostCommand, (CreatePostCommand, User)>
var stored     = new InRepo<Post>(postRepo);         // an IEffect<Post>
```

That is what the decomposition buys, and what a handler method takes away:

- **Reuse a step.** `InRepo<T>` serves every use case that stores a `T`. `Validated<T>` serves every use case that validates one.
- **Test a step alone.** Each primitive is one behaviour method. A test double is a class returning a value — no mocking framework, no pipeline to spin up.
- **Wrap a step.** A decorator goes around any link without touching the link or the rest of the flow.
- **Extend with your own types.** `WithAuthor : CraftEnvelope<…>` is accepted wherever an `ICraft` is, and composes with everything here.
- **Nothing runs until you ask.** Building the flow allocates and computes nothing. The work starts at `Yield()`.

### When it does not fit

A use case of three lines does not need to be decomposed; four objects to store one row is worse than one method. The gain arrives when a use case has grown enough that reading it means separating what belongs together from what is incidental.

Also worth knowing before adopting:

- **Everything is `Task`-based.** A synchronous domain pays for state machines it does not need.
- **There is no compensation.** If a late step throws, an earlier `Effect` has already written and there is no rollback ([What is missing](#what-is-missing)).
- **`net9.0` only, and 0.x**, so names still move between releases.

The question that decides it: should a use case be a sequence of named things, or a procedure? For the second, everything here is in the way.

## Principle

A use case is a flow of objects. Four primitives carry it:

| Primitive   | Shape      | Role |
|-------------|------------|------|
| **Seed**    | `() → T`   | Origin of a flow — produces the initial value |
| **Craft**   | `T → T'`   | Transforms the value; holds the actual use case logic |
| **Effect**  | `T → T`    | Acts on the value and passes it on unchanged — stores, indexes, writes |
| **Trigger** | `() → ()`  | Fires independently of the value — events, notifications |

Objects are named after their result, never after an activity. `WithAuthor` is a value that has an author, `InRepo` is the state after storing, `SanitizedHtml` is html that has been sanitized. There is no `HandlePost`, no `StorePost`, no `IsValid`.

Composition is decoration. Each call wraps what came before and returns a new object:

```csharp
var seed     = 42.AsSeed();                          // AsSeed<int>
var crafted  = seed.Craft(new AsCraft<int, string>(x => x.ToString()));   // CraftedSeed<int, string>
var effected = crafted.Effect(new AsEffect<string>(Console.WriteLine));   // SeedLink<string>
```

The type of a chain says what the chain is made of. Nothing collapses into a lambda.

## When code runs

**Building objects runs nothing. Code runs at the materializing call.**

| Type | Materializes with |
|---|---|
| `ISeed<T>` | `Yield()` — one value |
| `IFlow<T>` | `Yield(ct)` — an `IAsyncEnumerable<T>`, run by enumerating it |
| `ICraft<In,Out>` | `Yield(input)` |
| `IEffect<T>` | `Fire(value)` |
| `ITrigger` | `Act()` |

```csharp
var flow =
    postId.AsSeed()
        .Craft(new FromRepo<Post>(postRepo))    // nothing read
        .Effect(new RaisedLikeCount(postRepo)); // nothing written

var post = await flow.Yield();                  // both happen here
```

A chain of ten steps that is never yielded costs ten allocations and no work. Building one in a branch that turns out to be unused costs the allocations alone.

## Cardinality is the type

There are no booleans in a flow, and no `null`. A decision is a difference in how many values there are, and that difference lives in the type:

| Type | Values |
|---|---|
| `ISeed<T>` | exactly one |
| `IFlow<T>` | 0..n, and the whole chain runs once per value |
| `IOptional<T>` (Tonga) | 0..1 |

Crossing between them is a named step, never an implicit unwrap:

| From | To | Step |
|---|---|---|
| `ISeed<IEnumerable<T>>` | `IFlow<T>` | `Spread<T>` |
| `IObservable<T>` | `IFlow<T>` | `ObservedFlow<T>` |
| `IAsyncEnumerable<T>` | `IFlow<T>` | `AsFlow<T>` |
| `IFlow<T>` | `ISeed<IOptional<T>>` | `FirstOf<T>`, `LastOf<T>` |
| `IFlow<T>` | `ISeed<IEnumerable<T>>` | `Drained<T>` |

`FirstOf` and `LastOf` hand back an `IOptional<T>` because a flow may spawn nothing, and saying so in the type is the alternative to returning `null`.

```csharp
var flow =
    eventBus.Stream("post-events").AsFlow()
        .Craft(new EnrichedWithAuthor(userRepo))
        .Effect(new InSearchIndex(searchIndex))
        .Trigger(new Published<EventProcessed>(outboundBus));

await foreach (var processed in flow.Yield(cancellationToken)) { … }
```

The subscription is released when the observable completes, when the token is cancelled, or when the consumer stops enumerating. No explicit unsubscribe.

To select instead of branching, `SeedSwitch` takes the first case whose fact holds:

```csharp
var post = await new SeedSwitch<Post>(
        (currentUser.Owns(postId), new PostFromRepo(postId, postRepo)),   // IFact
        (currentUser.IsAdmin,      new PostFromRepo(postId, postRepo))
    ).Yield();
```

An overload takes `Func<bool>` for conditions that are not yet facts. Tonga's `AsFact` caches its answer; `Rechecked` asks anew on every yield.

## Composition: constructors or smarts

Every primitive can be built by constructor:

```csharp
new SeedLink<string>(
    new CraftedSeed<int, string>(
        new AsSeed<int>(42),
        new AsCraft<int, string>(x => x.ToString())
    ),
    new AsEffect<string>(Console.WriteLine)
).Yield();
```

or through extensions, which are named `…Smarts` after Tonga's convention — `SeedSmarts`, `CraftSmarts`, `EffectSmarts`, `TriggerSmarts`, `FlowSmarts` — and arrive with the `using` of their namespace:

```csharp
42.AsSeed()
  .Craft(x => x.ToString())
  .Effect(Console.WriteLine)
  .Yield();
```

Both build the same objects. The nested form reads from the inside out and puts the last step first, and requires the type arguments to be spelled out. The chained form follows execution order and infers them.

**An extension wraps and returns; it computes nothing.** Its body is one `new` and nothing else — no condition, no loop, no state:

```csharp
public static ISeed<TCrafted> Craft<TSeed, TCrafted>(this ISeed<TSeed> origin, Func<TSeed, TCrafted> craft)
    => origin.Craft(new AsCraft<TSeed, TCrafted>(craft));
```

EO forbids static methods because they carry behaviour belonging to no object. An extension under this rule carries none: the behaviour lives in `AsCraft`, a decorable and replaceable class. Nothing is decided in the extension, so there is nothing inheritance could concern.

Each interface has the same three combinators in a synchronous and an asynchronous overload, on `ISeed`, `ICraft`, `IEffect`, `ITrigger` and `IFlow` alike:

```csharp
.Trigger(Action)          .Trigger(Func<Task>)
.Effect(Action<T>)        .Effect(Func<T, Task>)
.Craft(Func<T, TOut>)     .Craft(Func<T, Task<TOut>>)
```

## Compared to one method

The usual shape:

```csharp
public async Task<Post> CreatePost(string body, string userId)
{
    var command = JsonSerializer.Deserialize<CreatePostCommand>(body);
    if (!validator.Validate(command).IsValid)
        throw new ValidationException();
    var author = await userRepo.Find(userId) ?? throw new NotFoundException();
    var post = new Post(command.Text, author);
    await postRepo.Save(post);
    await searchIndex.Add(post);
    await eventBus.Publish(new PostCreated(post.Id));
    foreach (var follower in await followRepo.Followers(userId))
        await pushClient.Notify(follower, post.Id);
    return post;
}
```

Everything here is reachable only through `CreatePost`. Storing a post, indexing it and notifying followers are three capabilities that the next use case will write again. Testing the validation means constructing four repositories.

The same use case as objects:

```csharp
var post =
    await new SeedFromJson<CreatePostCommand>(body)
        .Effect(new Validated<CreatePostCommand>(validator))
        .Craft(new WithAuthor(userId, userRepo))
        .Craft(new AsPost())
        .Effect(new InRepo<Post>(postRepo))
        .Effect(new InSearchIndex(searchIndex))
        .Trigger(new Published<PostCreated>(eventBus))
        .Trigger(new FollowerNotification(userId, followRepo, pushClient))
        .Yield();
```

`InRepo<T>`, `InSearchIndex`, `Published<T>` and `Validated<T>` are now available to every other use case, and each can be tested with the one thing it needs.

## Flow examples

### Like a post

```csharp
var post = await postId.AsSeed()
    .Craft(new FromRepo<Post>(postRepo))
    .Effect(new RaisedLikeCount(postRepo))
    .Effect(new Interaction(userId, analyticsRepo))
    .Trigger(new Published<PostLiked>(eventBus))
    .Trigger(new AuthorNotification(postRepo, pushClient))
    .Yield();
```

### Get a feed

```csharp
var feed = await userId.AsSeed()
    .Craft(new FollowedUserIds(followRepo))
    .Craft(new RecentPosts(postRepo, since: DateTimeOffset.UtcNow.AddDays(-2)))
    .Craft(new ByRelevance(rankingService))
    .Craft(new Page<Post>(page, pageSize))
    .Effect(new FeedView(userId, analyticsRepo))
    .Yield();
```

### Index every post of a user

A craft that yields many becomes a flow, and the rest of the chain runs per post:

```csharp
await userId.AsSeed()
    .Craft(new RecentPosts(postRepo))       // ISeed<IEnumerable<Post>>
    .Spread()                               // IFlow<Post>
    .Effect(new InSearchIndex(searchIndex))
    .Drained()                              // ISeed<IEnumerable<Post>>
    .Yield();
```

### Comment on a post

```csharp
var comment = await new SeedFromJson<CommentCommand>(body)
    .Effect(new Validated<CommentCommand>(validator))
    .Craft(new HtmlSafe())
    .Craft(new WithMentions(userRepo))
    .Craft(new AsComment(userId))
    .Effect(new InRepo<Comment>(commentRepo))
    .Effect(new CommentCount(postRepo))
    .Trigger(new PostAuthorNotification(postRepo, pushClient))
    .Trigger(new MentionedUsersNotification(pushClient))
    .Trigger(new Published<CommentPosted>(eventBus))
    .Yield();
```

## Cases

`Cases<T0, T1, …>` is an effect over a discriminated union ([OneOf](https://github.com/mcintyre321/OneOf)). It fires the handler of the active case and passes the concrete type, not the union wrapper:

```csharp
var result = await new SeedFromJson<ModeratePostCommand>(body)
    .Craft(new FromRepo<Post>(postRepo))
    .Craft(new ModerationOutcome(moderationService))   // OneOf<Approved, Rejected>
    .Effect(new Cases<Approved, Rejected>(
        approved => new InRepo<Approved>(publishQueue),
        rejected => new AsEffect<Rejected>(r => notifyAuthor(r.Reason))
    ))
    .Yield();
```

Arities two through nine are covered, matching what OneOf provides.

## Core abstractions

| Interface | Method | Namespace with implementations |
|---|---|---|
| `ISeed<T>` | `Yield()` | `Eluvion.Seed` |
| `IFlow<T>` | `Yield(ct)` | `Eluvion.Flow` |
| `ICraft<In,Out>` | `Yield(input)` | `Eluvion.Craft` |
| `IEffect<T>` | `Fire(value)` | `Eluvion.Effect` |
| `ITrigger` | `Act()` | `Eluvion.Trigger` |

Each namespace carries an `As…` to build one from a lambda, an `…Envelope` to derive your own, and a `…Link` to chain two. `Eluvion.Craft`, `Eluvion.Effect` and `Eluvion.Trigger` add a `…Morph` for implicit conversion from a lambda.

Composition is objects too. `.Effect(…)` on a craft produces an `Effected`, `.Trigger(…)` a `Triggered`, and `.Craft(…)` on a seed a `CraftedSeed` — so a chain keeps its structure instead of collapsing into a closure.

## Relationship to Tonga

Three of the four primitives have a counterpart in Tonga:

| Eluvion | Tonga | Difference |
|---|---|---|
| `ISeed<T>.Yield()` | `IScalar<T>.Value()` | async, plus the combinators |
| `ICraft<In,Out>.Yield(in)` | `IPipe<In,Out>.Yield(in)` | async, plus the combinators |
| `ITrigger.Act()` | `ITap.Trigger()` | async, plus the combinators |
| `IEffect<T>` | — | no counterpart |

What Eluvion adds is the use case level: the flow ordering, the async, `IEffect`, and cardinality as a type. What it takes from Tonga is everything below that — `IFact` instead of `bool` in `SeedSwitch`, `IOptional` instead of `null` at the flow boundaries, `IText` and `IScalar` at the entry points, and the envelope and smarts patterns throughout.

## Design rules

[DESIGN_GUIDE.md](DESIGN_GUIDE.md) carries the rules a step has to follow: no bool, no null, no hidden logic, no control flow, names describe the result, one interface per class.

## What is missing

- **No error decorators.** Tonga has `RetryOnError`, `BackFalling` and `ExceptionSwap` for `IScalar`. There is no equivalent for a flow yet, so a failing step propagates as-is.
- **No compensation.** An `Effect` that has already written is not undone when a later step throws.
- **`Cases` is only an effect.** It cannot branch into a different type and carry the flow on.
- **No concurrency guards.** Objects here are not protected against concurrent use; synchronization lives outside.
- **`Nullable` is disabled** in the project file.
- **No release.** CI packs and publishes on a tag, and no tag exists.

## License

MIT. See [LICENSE](LICENSE).
