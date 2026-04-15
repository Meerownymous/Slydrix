# Eluvion Use Case Design – Compact LLM Guide

## Core Model

A use case is a **linear flow of objects**.

Seed → Craft → Craft → Effect → Trigger → Yield

No:
- if
- bool
- null

All logic is expressed via **data transformation + cardinality**.

---

## Step Semantics

### Craft

Pure transformation or selection.

- Transform: T → T'
- Select: T → 0..1 T
- Expand: T → 0..n T'

---

### Effect

T → T

- performs side effects (DB, cache)
- does NOT change the flow value
- must describe resulting state

Example:
.Effect(new InRepo<Post>(repo))

---

### Trigger

T → T

- external reactions (events, notifications)
- no data change

Example:
.Trigger(PublishedPostCreated)

---

### Seed

() → T

Entry into the flow.

---

## Cardinality = Logic

No booleans.

- if → 0..1 result
- optional → 0..1
- collection → n

Example:

static IEnumerable<Post> DeletableBy(User u, Post p)
{
    if (p.AuthorId == u.Id || u.IsAdmin)
        yield return p;
}

---

## Naming Rules

Always describe RESULT, never action.

Correct:
- NeedingEvaluation
- MatchingEntry
- SanitizedHtml
- WithMentions
- RelevantPosts
- PagedPosts

Incorrect:
- NeedsEvaluation
- IsValid
- StorePost
- HandleX

---

## Naming Patterns

- WithX → enriched object
- ToX / AsX → transformation
- Xing → resulting set
- InX → state after effect

---

## Validation

Attributes define rules.

Validation is executed explicitly:

.Craft(Validated<T>)

---

## Layering

HTTP → UseCase → Domain

Explicit mapping:

.Craft(ToCommand)

---

## Example: Create Post

var post = await new SeedFromJson<CreatePostRequest>(body)
    .Craft(ToCreatePostCommand)
    .Craft(Validated<CreatePostCommand>)
    .Craft(WithAuthor)
    .Craft(AsPost)
    .Effect(new InRepo<Post>(repo))
    .Effect(new InSearchIndex(index))
    .Trigger(PublishedPostCreated)
    .Trigger(NotifiedFollowers)
    .Yield();

---

## Mental Model

Not algorithms.

A pipeline of objects representing facts.

---

## Design Rules

1. No bool
2. No null
3. No hidden logic
4. No control flow
5. Steps are only:
   - Transform
   - Select
   - Expand
   - Effect
   - Trigger
6. Naming = result

---

## Summary

A use case is a pipeline of objects where logic is expressed through transformation and cardinality, not control flow.
