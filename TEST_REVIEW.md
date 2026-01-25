# 🧪 TEST REVIEW: Sprint 1 Task 1.2 Source Code Analysis

**Reviewer Role**: QA Tester  
**Date**: January 25, 2026  
**Component**: Database, API, Frontend Scaffolding  
**Status**: ⚠️ CRITICAL ISSUES FOUND - Ready for development testing after fixes

---

## Executive Summary

The implementation is **architecturally sound** but has **several critical issues** preventing successful compilation and runtime execution:

- ❌ **Type Mismatches**: Domain.fs defines `Language` field missing from LessonCreateDto
- ❌ **Compilation Errors**: SessionCreateDto has wrong data types (float vs int, missing StartedAt/EndedAt)
- ❌ **Logic Bugs**: TypingView calculates metrics incorrectly (uses int for float values)
- ❌ **Data Mapping Issues**: Lesson creation doesn't set Language field
- ⚠️ **Validation Issues**: LessonCreateDto missing required `Language` field validation

**Estimated Fix Time**: 30-45 minutes  
**Testing Recommendation**: Fix issues before local testing begins

---

## Critical Issues Found

### 1. ❌ CRITICAL: Domain Type Mismatch (Lesson Creation)

**File**: [src/Shared/Domain.fs](src/Shared/Domain.fs#L45-L50)

**Issue**: `LessonCreateDto` missing `Language` field, but `Lesson` record requires it.

```fsharp
type LessonCreateDto = {
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    TextContent: string        // ← Mapped to "Content" in Lesson
    Tags: string option        // ← Unused field
}
// MISSING: Language field
```

**Impact**:
- Cannot create lessons without Language
- Database constraint: `language NOT NULL`
- API POST /api/lessons will fail at creation time

**Expected Fix**:
```fsharp
type LessonCreateDto = {
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language          // ← ADD THIS
    TextContent: string
}
```

**Affected Code**:
- [src/Server/Database/LessonRepository.fs](src/Server/Database/LessonRepository.fs) - createLesson function (line ~95)
- [src/Server/Handlers/LessonHandler.fs](src/Server/Handlers/LessonHandler.fs) - postLesson validation (line ~85)
- [src/Client/ApiClient.fs](src/Client/ApiClient.fs) - createLesson function (line ~60)

---

### 2. ❌ CRITICAL: SessionCreateDto Type Mismatch

**File**: [src/Shared/Domain.fs](src/Shared/Domain.fs#L75-L83)

**Issue**: SessionCreateDto has wrong data types compared to SessionDto and API usage.

```fsharp
type SessionCreateDto = {
    LessonId: Guid
    StartedAt: DateTime          // ← Not captured in TypingView
    EndedAt: DateTime            // ← Not captured in TypingView
    Wpm: float                   // ← Should be int (calculation returns int)
    Cpm: float                   // ← Should be int
    Accuracy: float              // ✓ Correct
    ErrorCount: int              // ✓ Correct
    PerKeyErrors: Map<string, int>
}
```

**Problem 1 - Wrong Data Types**:
In [src/Client/Pages/TypingView.fs](src/Client/Pages/TypingView.fs#L125), metrics are calculated as int:
```fsharp
let wpm, cpm, accuracy, errorCount = calculateMetrics ...
let sessionDto: SessionCreateDto = {
    Wpm = wpm        // ← int being assigned to float field (compiler will auto-cast)
    Cpm = cpm        // ← int being assigned to float field
    Accuracy = double accuracy  // ← Correctly converted to float
    ...
}
```

**Problem 2 - Missing DateTime Capture**:
TypingView doesn't capture actual StartedAt/EndedAt times:
```fsharp
let sessionDto: SessionCreateDto = {
    LessonId = model.Lesson.Id
    StartedAt = ???  // ← MISSING - where is this set?
    EndedAt = ???    // ← MISSING - where is this set?
    ...
}
```

**Expected Fix**:
```fsharp
type SessionCreateDto = {
    LessonId: Guid
    // Remove StartedAt/EndedAt - calculated server-side
    Wpm: int         // ← Change to int
    Cpm: int         // ← Change to int
    Accuracy: float
    ErrorCount: int
    PerKeyErrors: Map<string, int>
}
```

And in TypingView.fs, remove the DateTime handling:
```fsharp
let sessionDto: SessionCreateDto = {
    LessonId = model.Lesson.Id
    Wpm = wpm
    Cpm = cpm
    Accuracy = accuracy
    ErrorCount = errorCount
    PerKeyErrors = model.Errors
}
```

**Affected Code**:
- [src/Client/Pages/TypingView.fs](src/Client/Pages/TypingView.fs#L120-L135)
- [src/Server/Handlers/SessionHandler.fs](src/Server/Handlers/SessionHandler.fs#L10-L40) - handler expects these fields

---

### 3. ❌ CRITICAL: LessonRepository.createLesson Missing Language

**File**: [src/Server/Database/LessonRepository.fs](src/Server/Database/LessonRepository.fs#L90-L115)

**Issue**: createLesson function doesn't set the `Language` field when creating a lesson record.

```fsharp
let createLesson (dto: LessonCreateDto) =
    async {
        use conn = DbContext.createConnection()
        do! conn.OpenAsync() |> Async.AwaitTask
        
        let query = """
            INSERT INTO lessons (title, difficulty, content_type, language, content, created_at, updated_at)
            VALUES (@title, @difficulty, @content_type, @language, @content, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING *
        """
        
        let newId = Guid.NewGuid()
        let param = {| 
            title = dto.Title
            difficulty = ...
            content_type = ...
            language = ???  // ← WHERE IS THIS SET?
            content = dto.TextContent
        |}
        // ...
    }
```

**Impact**:
- Database INSERT will fail (Language NOT NULL constraint)
- SQL parameter binding missing language value

**Expected Fix**:
Add `language = dto.Language |> string` to param object.

---

### 4. ⚠️ HIGH: TypingView Metrics Type Mismatch

**File**: [src/Client/Pages/TypingView.fs](src/Client/Pages/TypingView.fs#L36-L46)

**Issue**: calculateMetrics returns int for WPM/CPM but SessionCreateDto expects float.

```fsharp
let calculateMetrics ... =
    let wpm = if duration > 0.0 then int (...) else 0    // ← Returns int
    let cpm = if duration > 0.0 then int (...) else 0    // ← Returns int
    let accuracy = ...                                     // ← Returns float
    let errorCount = ...                                   // ← Returns int
    wpm, cpm, int accuracy, Map.count errors              // ← Tuple of (int, int, int, int)
```

**Later in SubmitSession**:
```fsharp
let wpm, cpm, accuracy, errorCount = calculateMetrics ...
let sessionDto: SessionCreateDto = {
    Wpm = wpm      // ← int → float conversion (will compile but confusing)
    Cpm = cpm      // ← int → float conversion
    Accuracy = double accuracy  // ← Explicit conversion
    ErrorCount = errorCount
    ...
}
```

**Issue**: Inconsistent type handling and float precision loss.

**Expected Fix**:
Calculate as float from the start:
```fsharp
let calculateMetrics ... =
    let wpm = if duration > 0.0 then (words / (duration / 60.0)) else 0.0
    let cpm = if duration > 0.0 then ((double input.Length) / (duration / 60.0)) else 0.0
    let accuracy = if input.Length = 0 then 100.0 else ...
    wpm, cpm, accuracy, Map.count errors
```

---

### 5. ⚠️ HIGH: TypingView Backspace Logic Bug

**File**: [src/Client/Pages/TypingView.fs](src/Client/Pages/TypingView.fs#L103-L115)

**Issue**: Backspace creates invalid string when at the last character.

```fsharp
| Backspace ->
    if model.TypingState = InProgress && model.CurrentCharIndex > 0 then
        let newInput = model.UserInput.[0..model.CurrentCharIndex - 2]  // ← Can fail!
        let newIndex = model.CurrentCharIndex - 1
        let newErrors = Map.remove newIndex model.Errors
        ...
```

**Problem**: String slicing with `.[0..n]` fails if `n < 0` or beyond bounds.

**Example**:
- User types 1 character: CurrentCharIndex = 1, UserInput = "a"
- User presses Backspace: newInput = "a".[0..-1] → StringIndexOutOfRangeException

**Expected Fix**:
```fsharp
| Backspace ->
    if model.TypingState = InProgress && model.CurrentCharIndex > 0 then
        let newIndex = model.CurrentCharIndex - 1
        let newInput = 
            if newIndex = 0 then ""
            else model.UserInput.[0..newIndex - 1]
        let newErrors = Map.remove newIndex model.Errors
        ...
```

---

### 6. ⚠️ HIGH: LessonCreateDto.Language Missing from Validation

**File**: [src/Server/Handlers/LessonHandler.fs](src/Server/Handlers/LessonHandler.fs#L210-L231)

**Issue**: Validation function doesn't validate Language field exists (and it doesn't exist in DTO currently).

```fsharp
and validateLessonCreateDto (dto: LessonCreateDto) : ValidationError list =
    [
        // Title validation
        if String.IsNullOrWhiteSpace dto.Title then
            { Field = "title"; Message = "Title is required" }
        // ... (other validations)
        // MISSING: Language validation
    ]
```

**Expected Fix** (after adding Language to DTO):
```fsharp
and validateLessonCreateDto (dto: LessonCreateDto) : ValidationError list =
    [
        if String.IsNullOrWhiteSpace dto.Title then
            { Field = "title"; Message = "Title is required" }
        elif dto.Title.Length > 100 then
            { Field = "title"; Message = "Title cannot exceed 100 characters" }
        
        if String.IsNullOrWhiteSpace dto.TextContent then
            { Field = "textContent"; Message = "Content is required" }
        elif dto.TextContent.Length > 5000 then
            { Field = "textContent"; Message = "Content cannot exceed 5000 characters" }
        
        // ADD THIS:
        if String.IsNullOrWhiteSpace (dto.Language.ToString()) then
            { Field = "language"; Message = "Language is required" }
    ]
```

---

### 7. ⚠️ MEDIUM: LessonRepository.updateLesson Doesn't Set Language

**File**: [src/Server/Database/LessonRepository.fs](src/Server/Database/LessonRepository.fs#L130-L160)

**Issue**: Similar to createLesson, updateLesson doesn't handle the Language field.

```fsharp
let updateLesson (id: Guid, dto: LessonCreateDto) =
    async {
        // ... connection setup ...
        let query = """
            UPDATE lessons 
            SET title = @title, difficulty = @difficulty, content_type = @content_type, content = @content
            WHERE id = @id
            RETURNING ...
        """
        // MISSING: language = @language in UPDATE
        let param = {| 
            id = id
            title = dto.Title
            difficulty = ...
            content_type = ...
            // MISSING: language = ...
            content = dto.TextContent
        |}
    }
```

**Impact**: Cannot update lessons properly; Language stays as original value.

**Expected Fix**: Add language to both UPDATE clause and param object.

---

### 8. ⚠️ MEDIUM: SessionHandler.validateSessionCreateDto Missing StartedAt/EndedAt Validation

**File**: [src/Server/Handlers/SessionHandler.fs](src/Server/Handlers/SessionHandler.fs#L80-L120)

**Issue**: Validation only checks numeric ranges, doesn't validate DateTimes.

```fsharp
and validateSessionCreateDto (dto: SessionCreateDto) : ValidationError list =
    [
        if dto.LessonId = Guid.Empty then
            { Field = "lessonId"; Message = "Lesson ID is required" }
        
        if dto.Accuracy < 0.0 || dto.Accuracy > 100.0 then
            { Field = "accuracy"; Message = "Accuracy must be between 0 and 100" }
        
        // MISSING: 
        // if dto.StartedAt >= dto.EndedAt then validation
        // if dto.EndedAt > DateTime.Now validation
    ]
```

**Expected Fix** (after removing DateTime from DTO):
This validation becomes irrelevant since server will generate timestamps.

---

### 9. ⚠️ MEDIUM: TypingView.PerKeyErrors Type Mismatch

**File**: [src/Client/Pages/TypingView.fs](src/Client/Pages/TypingView.fs#L15-20)

**Issue**: Model uses `Map<int, int>` for per_key_errors (position → count).

```fsharp
type Model = {
    ...
    Errors: Map<int, int>  // ← position → error count
}
```

But SessionCreateDto expects `Map<string, int>`:
```fsharp
type SessionCreateDto = {
    ...
    PerKeyErrors: Map<string, int>  // ← string key (why?)
}
```

**Problem**: Type mismatch in SubmitSession handler:
```fsharp
let sessionDto: SessionCreateDto = {
    ...
    PerKeyErrors = model.Errors  // ← Map<int, int> → Map<string, int> fails!
}
```

**Expected Fix**:
Change SessionCreateDto to use int keys:
```fsharp
type SessionCreateDto = {
    ...
    PerKeyErrors: Map<int, int>  // ← Consistent with client tracking
}
```

Or convert in TypingView if string keys needed:
```fsharp
PerKeyErrors = model.Errors |> Map.toList |> List.map (fun (k, v) -> (string k, v)) |> Map.ofList
```

---

### 10. ⚠️ LOW: Program.fs Migration Path Hard-Coded

**File**: [src/Server/Program.fs](src/Server/Program.fs#L40-45)

**Issue**: Migration file paths are relative and will fail if run from different directory.

```fsharp
let! result = DbContext.runMigrations()
```

In DbContext.fs:
```fsharp
let migration1 = 
    System.IO.File.ReadAllText(
        "./src/Server/Database/Migrations/001_CreateLessonsAndSessionsTables.sql"
    )
```

**Problem**: Path is relative to working directory. If run from different path, will throw FileNotFoundException.

**Expected Fix**:
```fsharp
let basePath = System.AppContext.BaseDirectory
let migration1Path = System.IO.Path.Combine(basePath, "Migrations/001_CreateLessonsAndSessionsTables.sql")
let migration1 = System.IO.File.ReadAllText(migration1Path)
```

Or use AppDomain.CurrentDomain.BaseDirectory.

---

### 11. ⚠️ LOW: No CORS Configuration in Program.fs

**File**: [src/Server/Program.fs](src/Server/Program.fs)

**Issue**: Mentioned "CORS + Gzip compression" in documentation but not implemented.

```fsharp
let app =
    application {
        url "http://0.0.0.0:5000"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip  // ✓ OK
        // MISSING: CORS configuration
    }
```

**Impact**: Frontend will get CORS errors when running on different port.

**Expected Fix**:
```fsharp
use_cors "default" (fun p ->
    p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
)
```

---

### 12. ⚠️ LOW: No Input Sanitation in Handlers

**File**: [src/Server/Handlers/LessonHandler.fs](src/Server/Handlers/LessonHandler.fs)

**Issue**: No protection against SQL injection via string input (though Dapper uses parameterized queries, good practice to sanitize).

```fsharp
let errors = validateLessonCreateDto dto
// Should also trim/normalize strings
```

**Risk**: Low (Dapper parameterized queries protect), but good practice.

**Recommendation**: Add .Trim() to string fields in validation.

---

## Non-Critical Issues & Observations

### 13. ℹ️ Code Quality: Inconsistent Error Message Formatting

**Files**: Multiple handlers

**Observation**: Some errors use description, others use exception messages directly.

```fsharp
// Inconsistent
Message = "Failed to retrieve lessons"  // Generic
Message = ex.Message                     // Exception-specific (potentially exposes internals)
```

**Recommendation**: Use generic messages in production, log exceptions separately.

---

### 14. ℹ️ Code Quality: SessionHandler Never Used

**File**: [src/Server/Handlers/SessionHandler.fs](src/Server/Handlers/SessionHandler.fs)

**Observation**: SessionHandler exists but is never registered in Program.fs routes.

```fsharp
// In Program.fs webApp:
POST >=> route "/api/sessions" >=> SessionHandler.postSession  // ✓ Present
GET >=> routef "/api/lessons/%O/sessions" SessionHandler.getSessionsByLesson  // ✓ Present
GET >=> route "/api/sessions/last" >=> SessionHandler.getLastSession  // ✓ Present
```

Actually, routes ARE present. ✓ OK on second check.

---

### 15. ℹ️ Frontend: StartScreen doesn't reload on nav back

**File**: [src/Client/App.fs](src/Client/App.fs#L45-60)

**Observation**: When navigating back to StartScreen from TypingView, lessons aren't reloaded.

```fsharp
| NavigateToStartScreen ->
    { model with CurrentPage = StartScreen },
    Cmd.ofMsg (StartScreenMsg StartScreen.Msg.LoadLessons)  // ✓ Actually does reload
```

Good - reload command is sent. ✓ OK.

---

## Testing Recommendations

### Before Local Testing:
1. ✅ Fix Domain.fs type definitions (add Language to LessonCreateDto)
2. ✅ Fix SessionCreateDto data types (Wpm/Cpm: int, remove StartedAt/EndedAt)
3. ✅ Fix TypingView.SubmitSession to match corrected SessionCreateDto
4. ✅ Fix TypingView.Backspace string slicing logic
5. ✅ Fix LessonRepository.createLesson to set Language parameter
6. ✅ Fix LessonRepository.updateLesson to set Language parameter
7. ✅ Add CORS configuration to Program.fs
8. ✅ Fix migration file path handling in DbContext.fs
9. ✅ Add Language validation to LessonHandler

### During Local Testing:
1. Test POST /api/lessons with valid LessonCreateDto
2. Test POST /api/lessons with missing Language (should reject)
3. Test TypingView complete workflow (type text, submit session)
4. Test Backspace at various positions
5. Test error responses (404, 400, 500)
6. Test database migration idempotency
7. Test CORS headers on cross-origin requests

### Post-Fix Compilation Check:
```bash
cd src/Server && dotnet build KeyboardTrainer.Server.fsproj
cd src/Client && dotnet build KeyboardTrainer.Client.fsproj
```

---

## Summary by Severity

| Severity | Count | Issues |
|----------|-------|--------|
| 🔴 Critical | 5 | Domain types, SessionCreateDto, Language not set, Type mismatches |
| 🟠 High | 3 | Backspace logic, Validation gaps, PerKeyErrors type |
| 🟡 Medium | 2 | updateLesson Language, DateTime validation |
| 🔵 Low | 2 | File paths, CORS config |

**Total Issues**: 12  
**Blockers**: 5  
**Test Readiness**: ❌ NOT READY - Must fix critical issues first

---

## Recommended Fix Order

1. **Phase 1** (10 min): Fix Domain.fs types
   - Add Language to LessonCreateDto
   - Fix SessionCreateDto types
   - Fix PerKeyErrors type

2. **Phase 2** (15 min): Fix Repository functions
   - Update createLesson
   - Update updateLesson
   - Validate all SQL parameters match DTO

3. **Phase 3** (10 min): Fix Frontend
   - TypingView.SubmitSession mapping
   - TypingView.Backspace logic
   - Metrics calculation types

4. **Phase 4** (5 min): Polish
   - Add CORS config
   - Fix file paths
   - Add Language validation

5. **Validation** (5 min):
   - Compile both projects
   - Check no type errors

**Est. Total Time**: 45 minutes

---

## Conclusion

✅ **Architecture**: Sound, properly layered  
✅ **Structure**: Well-organized, follows SAFE conventions  
❌ **Implementation**: Multiple critical type mismatches blocking execution  
⏱️ **Timeline**: 45 min fixes + 30 min testing = Ready by 10:00 AM

**Recommendation**: Fix all critical issues in Phase 1-2 before proceeding to local testing environment setup.
