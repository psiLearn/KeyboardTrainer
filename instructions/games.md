# games

## candy crush inspired game

play field is 15 x 15 and randomly filled with weighted colors:
    31% yellow
    27% red
    23% green
    19% blue
player white gem starts in the middle and can go left, right, up, or down.
the pressed letter must fit the gem color on the side:
    a for blue gem on the left
    s for green gem on the left
    d for red gem on the left
    f for yellow gem on the left
    ö for blue gem on the right
    l for green gem on the right
    k for red gem on the right
    j for yellow gem on the right
    q or p for blue gem above
    w or o for green gem above
    e or i for red gem above
    r or u for yellow gem above
    y or - for blue gem below
    x or . for green gem below
    c or , for red gem below
    v or m for yellow gem below
example: red gem on the left means press d.
example: green gem on the right means press l.
collapse neighbouring gems with the same color, bringing new gems from the top.
after a correct move, swap the player with the matched neighboring gem.
calculate neighbours and score from the moved colored gem at the player's previous position, not from the gem's old position.
animate the collapsing gems before the new gems appear.
point calculation is switchable:
    current/exponential mode: 2^(n-1)
    square mode: n^2

settings:
    show letters inside colored gems
    move rows on/off
    sounds on/off
    point calculation: 2^(n-1) or n^2
    interval time in milliseconds for each frame / gems moving one row
    settings are collapsable
    restart with r or space bar
    restart keeps the current settings
row movement:
    moving rows rotates existing gems instead of deleting one and creating a new one
    when rows move down, the bottom gem wraps to the top of its column
collapse refill:
    non-collapsed gems move down into gaps
    replacement gems enter from the top
score:
    keep track of level/page score for the current lesson page
    use level/page score for targetScore and finish conditions
    keep a separate game score that accumulates across restarted levels in the same game session
    each collapse adds the same points to level/page score and game score
    point calculation mode controls how the collapse size becomes points
    manual restart or auto-restart resets level/page score but keeps game score
    show both level/page score and game score in the top stats
    show a high score table at the side using game score, with the level/page score beside it
    store high scores in browser local storage per lesson
sounds:
    play sound for valid hit
    play sound for miss
    play sound for finish celebration

starting parameters:
    rows
    columns
    tickMs
    durationSeconds
    targetScore
    lives
    showLettersInGems
    moveRows
    scoreMode: exponential or square

game lessons:
    Gem Game: Still Rows + Letters
        moveRows false, showLettersInGems true, tickMs 850
    Gem Game: Still Rows
        moveRows false, showLettersInGems false, tickMs 850
    Gem Game: Slow Rows + Letters
        moveRows true, showLettersInGems true, tickMs 1600
    Gem Game: Medium Rows + Letters
        moveRows true, showLettersInGems true, tickMs 850
    Gem Game: Fast Rows + Letters
        moveRows true, showLettersInGems true, tickMs 400
    Gem Game: Slow Rows
        moveRows true, showLettersInGems false, tickMs 1600
    Gem Game: Medium Rows
        moveRows true, showLettersInGems false, tickMs 850
    Gem Game: Fast Rows
        moveRows true, showLettersInGems false, tickMs 400
