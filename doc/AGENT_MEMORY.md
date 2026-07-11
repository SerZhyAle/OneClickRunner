# Persistent Agent Memory - a method, not a feature

An AI assistant forgets everything between sessions. The codebase, the spec files, and git
history persist - but *how you like to work*, *why a past decision went the way it did*, and
*which corrections you already gave* do not. Re-teaching that every session is the single
biggest hidden tax on working with an agent.

The fix is a small, file-based memory the agent reads at the start of a session and writes to
as it learns. It is plain Markdown in a `memory/` directory under version control, so the
whole team shares one growing brain. No runtime feature is required: if your agent has native
memory, use it; if not, point the agent at this folder and tell it to read the index first.

The discipline is what makes it work - **write the right things, skip the derivable ones, and
keep an index honest.** This document is that discipline.

## The index and the entries

Two kinds of file:

- **`MEMORY.md`** - the *index*. One line per memory: `- [Title](file.md) - one-line hook`.
  It is always loaded into the agent's context, so it stays short (a couple hundred lines at
  most). It is a table of contents, never a place to write memory content.
- **`<type>_<slug>.md`** - one *entry* per file, with frontmatter and a body. The agent reads
  the index, decides which entries are relevant, and opens only those.

This split keeps the always-on cost tiny while letting the actual memories be as detailed as
they need to be.

## The four types

Every memory is one of four types. The type tells future-you *why the memory exists* and *how
to use it*.

1. **`user`** - who the person is: their role, goals, expertise, what they already know.
   Use it to pitch explanations at the right level. *Save when* you learn a durable fact about
   them ("ten years of backend, first time in this frontend"). *Use it* to tailor depth and
   analogy - never to judge.

2. **`feedback`** - how to work *here*, learned from corrections **and** confirmations.
   Corrections are loud ("no, don't do that"); confirmations are quiet ("yeah, that bundled PR
   was the right call") and just as important - without them the agent drifts away from
   approaches you already blessed and grows over-cautious. *Save when* you correct or confirm a
   non-obvious approach. *Use it* so the same guidance is never needed twice.

3. **`project`** - ongoing context not derivable from code or git: who is doing what, why, by
   when; an incident; a constraint behind a decision. These decay fast - keep them current and
   convert relative dates to absolute ones when saving ("Thursday" → `2026-03-05`).

4. **`reference`** - a pointer to information in an external system and its purpose: "pipeline
   bugs live in the INGEST tracker", "the latency dashboard at <url> is what on-call watches".
   *Use it* when the person references that system or the information likely lives there.

## Body structure

For `feedback` and `project` entries, lead with the rule or fact, then two lines:

- **Why:** the reason - usually a past incident or a strong stated preference. This is what
  lets future-you judge an edge case instead of following the rule blindly.
- **How to apply:** when the guidance actually kicks in.

`user` and `reference` entries are usually a sentence or two and need no fixed structure.

Link related entries inline with `[[slug]]`. A link to a slug you have not written yet is
fine - it marks something worth writing later.

## What NOT to save

The hard part of memory is restraint. Do **not** save:

- Code patterns, conventions, architecture, file paths, project structure - re-derivable by
  reading the repo. (Those belong in `CLAUDE.md` or the code itself.)
- Git history or who-changed-what - `git log` / `git blame` are authoritative.
- Debugging solutions or fix recipes - the fix is in the code; the context is in the commit.
- Anything already written in `CLAUDE.md`.
- Ephemeral task state - that belongs in a plan or a task list, not memory.

These exclusions hold **even when asked to save**. If asked to remember a PR list or an
activity summary, ask what was *surprising* or *non-obvious* about it, and keep only that.

The test: *will this still be true and useful three sessions from now, and could I not just
read it off the repo?* If no to either, don't save it.

## How to save (two steps)

1. Write the entry file with frontmatter:

   ```markdown
   ---
   name: short-kebab-slug
   description: one specific line - this is what future-you scans to judge relevance
   type: user | feedback | project | reference
   ---

   Rule or fact in one or two sentences.

   **Why:** the incident or preference behind it.
   **How to apply:** when this kicks in.
   ```

2. Add one pointer line to `MEMORY.md` under a topical heading. Never paste the body into the
   index.

Organize the index by topic, not by date. Before writing a new entry, check whether an
existing one should be *updated* instead - no duplicates.

## Memory is true-at-a-point-in-time

An entry that names a function, file, or flag is a claim that it existed *when written*. It may
have been renamed, removed, or never merged. Before acting on such a memory: if it names a
path, check the path exists; if it names a symbol or flag, grep for it. "Memory says X exists"
is not "X exists now." When a remembered fact conflicts with what you observe in the repo
today, trust the repo and fix or delete the stale entry.

Memories that summarize repo state (an architecture snapshot, an activity log) are frozen in
time. For *current* state, read the live code / working tree - not a recalled snapshot, and
not `git log` (which answers how you got here, not what is true now).

## Memory vs plan vs tasks

Three different persistence tools, often confused:

- **Memory** - survives across sessions. For durable, non-obvious facts about the person, the
  way of working, and the project's living context.
- **Plan** - aligns on an approach *before* a non-trivial task in the current session. Persist
  a change of approach by editing the plan, not by saving a memory.
- **Tasks / todo list** - tracks steps *within* the current session. Throw it away at the end.

If information is only useful for the task in front of you, it is not a memory.

Durable memory is for facts worth keeping; the in-flight state of one interrupted task is
not - and the two must not be mixed (a transient next-step never gets promoted into the
durable store). For a task that spans an interruption, bookend the session with a continuity
ritual: at each phase boundary and at task end, write a short structured handoff - goal, what
changed, decisions, open blockers, the single next step - and on resume, load the most recent
handoff before doing anything else. Keep these ephemeral and separate from long-term memory (a
scratch/handoff file or the plan itself, never `memory/`).

## Adapting it

- The four types are a strong default, not a law. If your team needs a fifth, add it - but
  resist turning memory into a wiki; the index is always-loaded and must stay short.
- No runtime memory feature? Keep `memory/` in the repo and add a line to `CLAUDE.md`: "At the
  start of a session, read `memory/MEMORY.md` and open any entry that looks relevant." That is
  the whole mechanism.
- Per-agent memory (optional): the default is one shared `memory/` - usually right, since it is
  the project's single brain. Only if several writing agents start crowding each other, give each
  its own `memory/<agent>/` and point each role brief at that path; otherwise keep the shared index.
