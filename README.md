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

### Why Eluvion

A use case tends to grow into a single method that does everything: loads data, validates, transforms, persists, notifies. Reading it means untangling what belongs together and what is just incidental noise.

Eluvion makes the *shape* of a use case visible. Each step is a named object with a single responsibility. The pipeline reads top to bottom — what comes in, what changes, what fires, what comes out. There is no hidden control flow.

Because each primitive is its own class, it can be tested in isolation, replaced without touching the rest of the flow, and reused across use cases. Decomposition stops being a refactoring task and becomes the default way of writing.

---

## Subscription

`Subscription<T>` is a seed that drives the pipeline from an `IObservable<T>`. The full pipeline runs once for every emitted value. `.Yield()` blocks until the observable completes or the cancellation token is cancelled, and returns the last processed value.

```csharp
await new Subscription<DomainEvent>(eventBus.Stream("post-events"), cancellationToken)
    .Craft(new EnrichedWithAuthor(userRepo))
    .Effect(new InSearchIndex(searchIndex))
    .Trigger(new Published<EventProcessed>(outboundBus))
    .Yield();
```

The subscription is cleaned up automatically — no explicit unsubscribe required.

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
