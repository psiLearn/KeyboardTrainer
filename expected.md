# Gem Game Lesson Flow Report

Test date: 2026-04-26

## Summary

The game behaves as expected when started from the real lesson card `Gem Game: Color Rush`.

The app opened the lesson as a game iframe, loaded the lesson config, rendered a 15 x 15 board, displayed the switched right-side key mapping, and accepted the correct side/color key. Current rules use weighted gem colors, connected same-color collapse, `2^(n-1)` scoring for `n` collapsed gems, startup settings for gem letters and row movement, local high scores, collapsible settings, optional generated sounds, and four-way movement from the center.

## Playwright MCP Status

I attempted to use Playwright MCP directly, but the MCP browser launcher still fails before opening a page:

`Chromium distribution 'chrome' is not found at C:/Users/siebk/AppData/Local/Google/Chrome/Application/chrome.exe`

I also ran `npx playwright install chrome`, but it failed because installing branded Chrome needs administrator privileges. Because MCP could not launch, I used Playwright Chromium to perform the same browser flow and recorded the MCP limitation here. The limitation is tooling/environment related, not a game failure.

## Lesson Used

| Field | Value |
| --- | --- |
| Lesson | `Gem Game: Color Rush` |
| Lesson id | `90f024fa-0e13-4cb7-a9ce-ce32d8ccbe29` |
| Content type | `gem_game` |
| Config | `{"rows":15,"columns":15,"tickMs":850,"durationSeconds":75,"targetScore":500,"lives":10,"showLettersInGems":false,"moveRows":true}` |
| Game iframe | `/games/gem-game/index.html?...` |

## Game Lesson Variants

The app now seeds eight additional game lessons so learners can start from a specific movement and letter-visibility mode.

| Lesson | Move rows | Show letters | Tick interval |
| --- | --- | --- | --- |
| `Gem Game: Still Rows + Letters` | No | Yes | 850 ms |
| `Gem Game: Still Rows` | No | No | 850 ms |
| `Gem Game: Slow Rows + Letters` | Yes | Yes | 1600 ms |
| `Gem Game: Medium Rows + Letters` | Yes | Yes | 850 ms |
| `Gem Game: Fast Rows + Letters` | Yes | Yes | 400 ms |
| `Gem Game: Slow Rows` | Yes | No | 1600 ms |
| `Gem Game: Medium Rows` | Yes | No | 850 ms |
| `Gem Game: Fast Rows` | Yes | No | 400 ms |

## Color Distribution And Scoring

| Gem color | Generation weight |
| --- | --- |
| Yellow | 31% |
| Red | 27% |
| Green | 23% |
| Blue | 19% |

| Collapsing gems `n` | Points `2^(n-1)` |
| --- | --- |
| 1 | 1 |
| 2 | 2 |
| 3 | 4 |
| 4 | 8 |
| 5 | 16 |

Valid moves collapse the connected neighboring group of the same color, using horizontal and vertical neighbors.

## Screenshots

Start screen with the lesson:

![Start screen lesson](output/playwright/lesson-flow-start.png)

Game opened from the lesson after the right-side key switch:

![Game initial after switch](output/playwright/lesson-flow-after-switch-initial.png)

After pressing the valid switched right-side key:

![After switched right key](output/playwright/lesson-flow-after-switch-right-key.png)

After connected same-color collapse:

![Connected collapse after hit](output/playwright/gem-connected-collapse-after-hit.png)

Collapse animation active:

![Collapse animation active](output/playwright/gem-collapse-animation-active.png)

After collapse animation and replacement:

![Collapse animation after](output/playwright/gem-collapse-animation-after.png)

Settings before changes:

![Game settings initial](output/playwright/gem-settings-initial.png)

Letters shown inside gems:

![Letters inside gems](output/playwright/gem-settings-letters-on.png)

Row movement interval changed to 2000 ms:

![Interval setting at 2000 ms](output/playwright/gem-settings-interval-2000.png)

Starting parameters with letters enabled and row movement disabled:

![Starting parameters no row movement](output/playwright/gem-starting-params-no-move.png)

Row movement enabled again from settings:

![Move rows enabled](output/playwright/gem-move-rows-enabled.png)

Space restart before pressing Space:

![Space restart before](output/playwright/gem-space-restart-before.png)

Space restart after pressing Space:

![Space restart after](output/playwright/gem-space-restart-after.png)

Restart keeps changed settings before pressing Space:

![Restart keep settings before](output/playwright/gem-restart-keep-settings-before.png)

Restart keeps changed settings after pressing Space:

![Restart keep settings after](output/playwright/gem-restart-keep-settings-after.png)

After switching `k` and `l` colors:

![K/L color switch](output/playwright/gem-k-l-switch.png)

High-score table, sound setting, and collapsed settings:

![High score and sound settings](output/playwright/gem-high-score-sound-settings.png)

15 x 15 board with center start and vertical movement:

![15 x 15 up/down movement](output/playwright/gem-15x15-up-down.png)

Expanded game layout with only the top Keyboard Trainer bar:

![Expanded game layout](output/playwright/game-more-space-layout.png)

Mobile fit check:

![Mobile fit](output/playwright/lesson-flow-mobile.png)

## Flow

```mermaid
flowchart TD
  A[Open http://localhost] --> B[Click Gem Game: Color Rush lesson]
  B --> C[App opens game iframe with lesson config]
  C --> D[Game renders 15 x 15 board]
  D --> E[Find matched neighboring same-color group]
  E --> F[Press matching side and color key]
  F --> G[Connected same-color gems get collapse animation]
  G --> H[Animated gems disappear and replacements come from top]
  H --> I[Score gain = 2^(n-1)]
  I --> J[Turn on gem letters]
  J --> K[Colored gems display their matching letters]
  K --> L[Set row interval to 2000 ms]
  L --> M[Game confirms new row movement interval]
  M --> N[Start with moveRows false from config]
  N --> O[Board stays still across ticks]
  O --> P[Enable Move rows setting]
  P --> Q[Board starts moving again]
  Q --> R[Press Space]
  R --> S[Game restarts from lesson config]
```

Note: the on-screen key for right blue is `ö`.

## Key Mapping

| Letter | Gem color | Direction |
| --- | --- | --- |
| `a` | Blue | Left |
| `s` | Green | Left |
| `d` | Red | Left |
| `f` | Yellow | Left |
| `ö` | Blue | Right |
| `l` | Green | Right |
| `k` | Red | Right |
| `j` | Yellow | Right |
| `q` or `p` | Blue | Up |
| `w` or `o` | Green | Up |
| `e` or `i` | Red | Up |
| `r` or `u` | Yellow | Up |
| `y` or `-` | Blue | Down |
| `x` or `.` | Green | Down |
| `c` or `,` | Red | Down |
| `v` or `m` | Yellow | Down |

## Checks

| Check | Result | Evidence |
| --- | --- | --- |
| Starts from lesson card | Pass | Clicking `Gem Game: Color Rush` opened the game iframe. |
| Uses lesson config | Pass | Iframe URL includes rows `10`, columns `10`, tick `850`, duration `75`, target `500`, lives `10`. |
| Board size | Pass | Latest board measured 15 rows and 15 columns. |
| Gem colors | Pass | Initial board contained blue 30, yellow 30, red 22, green 18. |
| Desktop fit | Pass | Board width was 473 px. |
| Mobile fit | Pass | Board width was 311.28 px inside a 348 px iframe viewport. |
| Weighted color generation | Pass | A sampled board contained yellow 29, red 28, green 23, blue 20, matching the intended weighted shape rather than an even split. |
| Connected collapse | Pass | A blue group of 2 connected neighboring gems collapsed after pressing `a`. |
| Exponential scoring | Pass | The 2-gem collapse scored `2^(2-1) = 2`. |
| Collapse animation | Pass | A later run showed 4 connected green gems with `.collapsing-gem` before replacement; after animation the class count returned to 0. |
| Correct key movement | Pass | Pressing the matching side/color key moved the player into the collapsed neighboring group. |
| Settings panel | Pass | The game shows `Game Settings`, a letter toggle, and a row movement interval slider. |
| Letter overlay setting | Pass | Enabling the checkbox shows letters inside gems; latest 15 x 15 board showed 224 gem letters because the player cell does not show a letter. |
| Row interval setting | Pass | Moving the slider changed the interval from 850 ms to 2000 ms and showed `Row movement interval set to 2000 ms.` |
| Settings as starting parameters | Pass | Direct startup config with `showLettersInGems:true` and `moveRows:false` loaded those setting states before interaction. |
| Disable row movement | Pass | With `moveRows:false`, the board signature stayed identical after more than one tick while the timer continued. |
| Re-enable row movement | Pass | Checking `Move rows` changed the board signature on the next tick and showed `Rows will move.` |
| Lesson variants | Pass | Eight additional gem-game lessons were inserted into the running database with the requested movement and letter settings. |
| Space restart | Pass | The game key listener treats Space as a restart shortcut, alongside `R`, and the finish button says `Play Again (R or Space)`. |
| Restart keeps settings | Pass | Restart creates a fresh board and score state while preserving letter visibility, row movement, and interval settings. |
| `k`/`l` color switch | Pass | Right green now shows `l` and accepts `l`; right red now shows `k` and accepts `k`. |
| Collapsible settings | Pass | Settings start collapsed, expand with the settings header, and keep the setting controls inside the expanded body. |
| Sound setting | Pass | The expanded settings body includes a `Sounds` checkbox; disabling it keeps the state off while play continues. |
| High-score table | Pass | The side table starts empty, then records completed round score, hits, misses, and timestamp in local storage. |
| Center start | Pass | Player starts at row 7, column 7 on the 15 x 15 field. |
| Four-way movement | Pass | Up and down controls work in addition to left/right; latest run moved up with `w` and down with `c`. |
| Expanded game layout | Pass | Game mode keeps the top navbar, removes the footer and outer game header, and lets the iframe fill the remaining viewport. |
| Console errors | Pass | No browser console errors were captured during the final lesson flow. |
| Page errors | Pass | No page errors were captured during the final lesson flow. |

## Key Evidence

Initial state:

```json
{
  "rows": 15,
  "columns": 15,
  "playerColumn": 7,
  "playerRow": 7,
  "colorCounts": {
    "yellow": 29,
    "red": 28,
    "green": 23,
    "blue": 20
  },
  "controls": [
    "Left side: a blue, s green, d red, f yellow",
    "Right side: ö blue, l green, k red, j yellow"
  ],
  "hint": "left blue: a | right yellow: j",
  "score": "0",
  "lives": "10",
  "hitsMisses": "0 / 0"
}
```

After valid key:

```json
{
  "collapsedColor": "blue",
  "collapsedGems": 2,
  "pressedKey": "a",
  "playerColumn": 4,
  "score": "2",
  "lives": "10",
  "hitsMisses": "1 / 0",
  "lastHint": "Moved left. Collapsed 2 gems! +2 points."
}
```

After enabling settings:

```json
{
  "settingsText": "Game Settings\nShow letters inside colored gems\nMove rows\nRow movement interval: 2000 ms",
  "showLettersChecked": true,
  "moveRowsChecked": true,
  "intervalValue": "2000",
  "gemLetterCount": 224,
  "lastHint": "Row movement interval set to 2000 ms."
}
```

Starting parameters and row movement toggle:

```json
{
  "config": {
    "rows": 15,
    "columns": 15,
    "tickMs": 850,
    "durationSeconds": 75,
    "targetScore": 500,
    "lives": 10,
    "showLettersInGems": true,
    "moveRows": false
  },
  "initial": {
    "showLettersChecked": true,
    "moveRowsChecked": false,
    "gemLetterCount": 224,
    "time": "01:15"
  },
  "afterNoMoveTick": {
    "moveRowsChecked": false,
    "time": "01:13"
  },
  "afterMoveEnabled": {
    "moveRowsChecked": true,
    "time": "01:12",
    "lastHint": "Rows will move."
  },
  "boardStayedStillWithMoveRowsFalse": true,
  "boardMovedAfterEnablingMoveRows": true
}
```

Lesson variants and Space restart:

```json
{
  "variantCount": 8,
  "openedLesson": "Gem Game: Still Rows + Letters",
  "initial": {
    "showLettersChecked": true,
    "moveRowsChecked": false,
    "intervalValue": "850",
    "gemLetterCount": 224,
    "lives": "1",
    "isFinished": false
  },
  "afterGameOver": {
    "lives": "0",
    "isFinished": true,
    "finishButton": "Play Again (R or Space)"
  },
  "afterSpaceRestart": {
    "showLettersChecked": true,
    "moveRowsChecked": false,
    "lives": "1",
    "isFinished": false
  },
  "spaceRestartPassed": true
}
```

Restart after changing settings:

```json
{
  "beforeRestart": {
    "showLettersChecked": true,
    "moveRowsChecked": false,
    "intervalValue": "1600"
  },
  "afterRestart": {
    "showLettersChecked": true,
    "moveRowsChecked": false,
    "intervalValue": "1600",
    "score": "0"
  },
  "restartKeptSettings": true
}
```

After switching right-side `k` and `l` colors:

```json
{
  "controls": "Right side: ö blue, l green, k red, j yellow",
  "rightGreen": {
    "hint": "right green: l",
    "pressed": "l",
    "hitsMisses": "1 / 0"
  },
  "rightRed": {
    "hint": "right red: k",
    "pressed": "k",
    "hitsMisses": "1 / 0"
  }
}
```

High-score table and sound setting:

```json
{
  "initial": {
    "settingsClass": "gem-game-settings collapsed",
    "settingsBodyExists": false,
    "highRows": [["Finish a round to record a score."]]
  },
  "expanded": {
    "settingsBodyExists": true,
    "soundChecked": true
  },
  "soundOff": {
    "soundChecked": false
  },
  "finished": {
    "highRows": [["1", "0", "0/1", "2026-04-26 21:49"]],
    "localStorage": "0|0|1|2026-04-26 21:49"
  }
}
```

15 x 15 field and vertical movement:

```json
{
  "initial": {
    "rows": 15,
    "columns": 15,
    "player": {
      "row": 7,
      "column": 7
    },
    "activeTargets": 4,
    "gemLetterCount": 224
  },
  "up": {
    "hint": "up green: w/o",
    "pressed": "w",
    "player": {
      "row": 6,
      "column": 7
    },
    "hitsMisses": "1 / 0"
  },
  "down": {
    "hint": "down red: c/,",
    "pressed": "c",
    "player": {
      "row": 8,
      "column": 7
    },
    "hitsMisses": "1 / 0"
  }
}
```

Full machine-readable result:

`output/playwright/lesson-flow-after-switch-result.json`

Settings result:

`output/playwright/gem-settings-result.json`

Starting parameters result:

`output/playwright/gem-starting-parameters-result.json`

Lesson variants and Space restart result:

`output/playwright/gem-lesson-variants-space-restart-result.json`

Restart keeps settings result:

`output/playwright/gem-restart-keep-settings-result.json`

`k`/`l` color switch result:

`output/playwright/gem-k-l-switch-result.json`

High-score, sound, and collapsible settings result:

`output/playwright/gem-high-score-sound-settings-result.json`

15 x 15 and up/down movement result:

`output/playwright/gem-15x15-up-down-result.json`

Connected collapse result:

`output/playwright/gem-connected-collapse-result.json`

Collapse animation result:

`output/playwright/gem-collapse-animation-result.json`

Expanded layout result:

`output/playwright/game-more-space-layout.json`

## Conclusion

The game behaves as expected when started from the lesson. The pressed letter must match both the movement direction and the adjacent gem color. The board is now 15 x 15, and the player starts in the center. The right-side mapping is `ö` blue, `l` green, `k` red, and `j` yellow. Up uses `q/p`, `w/o`, `e/i`, `r/u`; down uses `y/-`, `x/.`, `c/,`, `v/m`. The latest movement run successfully moved up with `w` and down with `c`. The settings also work as startup parameters and runtime controls: letters can be shown inside colored gems, rows can be stopped or restarted, sounds can be muted, settings can collapse, and the row movement interval can be changed while the game is running. The eight additional lesson variants load the requested settings, the side table keeps high scores per lesson in browser local storage, and Space restarts the game like `R`.

The remaining issue is only with Playwright MCP startup on this machine. MCP needs Chrome installed at the expected path or configured to use an available browser such as Edge or Playwright Chromium.


