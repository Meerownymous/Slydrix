# Eluvion

An elegant C# library for decomposing use cases into composable, reusable parts.

Every flow is built from four primitives:

| Primitive   | Role                                                                                    |
|-------------|-----------------------------------------------------------------------------------------|
| **Seed**    | Origin of a flow — produces the initial value                                           |
| **Trigger** | Fires independently of the data flowing through                                         |
| **Effect**  | Receives the current value, acts on it, passes it unchanged                             |
| **Craft**   | Transforms the current value into a new one, will typically hold the main usecase logic |

Flows start acting when calling `.Yield()`, which will activate the whole pipeline and result in the final value.

Every primitive can be constructed directly (`new AsCraft<int, string>(x => x.ToString())`) or through its extensions, which are named `…Smarts` after Tonga's convention (`SeedSmarts`, `CraftSmarts`, `EffectSmarts`, `TriggerSmarts`, `FlowSmarts`) and arrive with the `using` of their namespace. An extension wraps and returns; it computes nothing.

```csharp
42.AsSeed()                          // ISeed<int>
  .Craft(x => x.ToString())          // instead of .Craft(new AsCraft<int, string>(…))
  .Effect(Console.WriteLine)
  .Yield();
```

### Why Eluvion

A use case tends to grow into a single method that does everything: loads data, validates, transforms, persists, notifies. Reading it means untangling what belongs together and what is just incidental noise.

Eluvion makes the *shape* of a use case visible. Each step is a named object with a single responsibility. The pipeline reads top to bottom — what comes in, what changes, what fires, what comes out. There is no hidden control flow.

Because each primitive is its own class, it can be tested in isolation, replaced without touching the rest of the flow, and reused across use cases. Decomposition stops being a refactoring task and becomes the default way of writing.

---



## Social Network — Flow Examples

### 1. Create Post

```csharp
var post = await new SeedFromJson<CreatePostCommand>(requestBody)
    .Effect(new Validated<CreatePostCommand>(validator))
    .Craft(new WithAuthor(userId, userRepo))
    .Craft(new AsPost())
    .Effect(new InRepo<Post>(postRepo))
    .Effect(new InSearchIndex(searchIndex))
    .Trigger(new Published<PostCreated>(eventBus))
    .Trigger(new FollowerNotification(userId, followRepo, pushClient))
    .Yield();
```

---

### 2. Like a Post

```csharp
var post = await postId.AsSeed()
    .Craft(new FromRepo<Post>(postRepo))
    .Effect(new RaisedLikeCount(postRepo))
    .Effect(new Interaction(userId, analyticsRepo))
    .Trigger(new Published<PostLiked>(eventBus))
    .Trigger(new AuthorNotification(postRepo, pushClient))
    .Yield();
```

---

### 3. Delete Post

```csharp
var deleted = await new SeedIf<Post>(
        (() => currentUser.Owns(postId), new FromRepo<Post>(postRepo)),
        (() => currentUser.IsAdmin,      new FromRepo<Post>(postRepo))
    )
    .Effect(new Absent<Post>(postRepo))
    .Effect(new AbsentFromIndex(searchIndex))
    .Trigger(new Published<PostDeleted>(eventBus))
    .Trigger(new AsTrigger(() => auditLog.Record(postId, currentUser)))
    .Yield();
```

---

### 4. Get Feed

```csharp
var feed = await userId.AsSeed()
    .Craft(new FollowedUserIds(followRepo))
    .Craft(new RecentPosts(postRepo, since: DateTimeOffset.UtcNow.AddDays(-2)))
    .Craft(new ByRelevance(rankingService))
    .Craft(new Page<Post>(page, pageSize))
    .Effect(new FeedView(userId, analyticsRepo))
    .Yield();
```

---

### 5. Comment on Post

```csharp
var comment = await new SeedFromJson<CommentCommand>(requestBody)
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

## Flows — a source of many values

A seed yields one value. A source of many is an `IFlow<T>`, carrying the same four primitives: the whole chain runs once per value.

`ObservedFlow<T>` spawns everything an `IObservable<T>` emits:

```csharp
var flow =
    eventBus.Stream("post-events").AsFlow()
        .Craft(new EnrichedWithAuthor(userRepo))
        .Effect(new InSearchIndex(searchIndex))
        .Trigger(new Published<EventProcessed>(outboundBus));

await foreach (var processed in flow.Yield(cancellationToken)) { … }
```

`Yield()` hands back an `IAsyncEnumerable<T>` and runs nothing until it is enumerated. The subscription is released when the observable completes, when the token is cancelled, or when the consumer stops enumerating — no explicit unsubscribe required.

`Spread<T>` turns a seed that yielded many items into a flow, which is how a craft of cardinality `0..n` connects to the rest:

```csharp
var flow =
    userId.AsSeed()
        .Craft(new RecentPosts(postRepo))   // ISeed<IEnumerable<Post>>
        .Spread()                           // IFlow<Post>
        .Effect(new InSearchIndex(searchIndex));
```

### From a flow back to a seed

Three seeds say what to take from a flow. A flow can spawn nothing, so `FirstOf` and `LastOf` hand back Tonga's `IOptional<T>` rather than a null:

| Seed | Value |
|---|---|
| `FirstOf<T>` | `IOptional<T>` — the first value, stops the flow there |
| `LastOf<T>` | `IOptional<T>` — the last value |
| `Drained<T>` | `IEnumerable<T>` — everything the flow spawned |

```csharp
var last = await flow.LastOf(cancellationToken).Yield();
last.IfHas(evt => log.Record(evt));
```

---

## Cases

`Cases<T0, T1, ...>` is an effect for discriminated unions (`OneOf<T0, T1, ...>`). It inspects the active case and fires the matching handler, passing the concrete type — not the union wrapper.

```csharp
var result = await new SeedFromJson<ModeratePostCommand>(requestBody)
    .Craft(new FromRepo<Post>(postRepo))
    .Craft(new ModerationOutcome(moderationService))   // returns OneOf<Approved, Rejected>
    .Effect(new Cases<Approved, Rejected>(
        approved => new InRepo<Approved>(publishQueue),
        rejected => new AsEffect<Rejected>(r => notifyAuthor(r.Reason))
    ))
    .Yield();
```

`Cases` supports up to ten type parameters, covering all arities provided by the [OneOf](https://github.com/mcintyre321/OneOf) library.

---
