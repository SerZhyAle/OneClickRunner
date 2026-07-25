# Strategic spec: T0003 - Safe yt-dlp invocation (no shell string injection)

**Ticket:** T0003
**Proposal:** C3
**Status:** Implemented
**Priority:** 80
**Date:** 2026-07-11
**Tier:** Easy
**Tactical plan:** `PLAN/T0003_safe-ytdlp-invocation/` (created by /spec-tech)

## 1. Problem
The yt-dlp special case builds a shell command by string-concatenating the user's link into
`cmd /k "cd /d "<downloads>" && yt-dlp <link>"`. A link containing shell metacharacters (for
example `x & del ...`) is interpreted by `cmd`, so the input is executed as a command line
rather than passed as a single argument. It is self-inflicted and local, but it is a real
injection and poor hygiene.

## 2. Goals
1. The entered link is passed to yt-dlp as a single argument, never interpreted by a shell.
2. The download still runs in the user's Downloads folder and stays visible (a window the user
   can watch), as today.

**Non-goals:**
- Adding format/output options or an existence check for yt-dlp - that is the larger T0013.

## 4. Current architecture context
The concatenation lives in `App.HandleCommandLineArgs` inside the `item.Path == "SPECIAL_YTDLP"`
branch (App.xaml.cs), which starts `cmd.exe` with `/k` and the composed string via
`UseShellExecute = true`.

## 5. Proposed approach
Stop composing a `cmd` string from the link. Invoke the yt-dlp executable directly with the
link supplied as a discrete argument and the working directory set to Downloads, so no shell
parses the input. If a visible console window is still wanted, keep it without routing the
untrusted link through `cmd`'s parser.

## 7. Risks
| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|--------|-----------|
| Losing the persistent `cmd /k` window changes UX (window closes at end) | Medium | User cannot read final output | Decide the window behaviour in the tactical step; keep output visible |

## 10. Links to other specs
- Superseded/extended by the first-class yt-dlp type: T0013.
- Shares the launcher once T0001 lands.

## 11. Done criteria (strategic)
1. A link containing `&`, `|`, or quotes is handed to yt-dlp verbatim and not executed by a shell.
2. A normal link still downloads into the Downloads folder.

## 12. Next step
Done; extended by the first-class type T0013.

**Result (2026-07-11):** Implemented in `ScenarioLauncher.LaunchYtDlp`. The raw link is never placed
on a command line: it is passed to `cmd` via the `OCR_YTDLP_LINK` environment variable and referenced
as `"%OCR_YTDLP_LINK%"`, so `&`, `|`, `<`, `>` inside it are literal (this also fixes the real-world
bug of `&`-bearing YouTube URLs breaking). Links containing `"` or control characters are rejected up
front (a valid URL has neither). The persistent, watchable `cmd /k` window is preserved
(`UseShellExecute = false`). yt-dlp presence is checked on PATH with a clear error if absent. Release
build: 0 errors.
