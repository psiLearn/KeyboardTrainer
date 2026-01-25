# 🧪 TESTER REPORT - Sprint 1 Task 1.2

**Role**: QA Tester  
**Status**: ⛔ Code Review Complete - **12 Issues Found, 5 Critical**  
**Recommendation**: **FIX BEFORE LOCAL TESTING**

---

## Quick Summary

### ✅ What Works Well
- Clean architecture with proper layering
- Good use of async/await patterns
- Proper error handling structure
- Type-safe approach with F# records

### ❌ What Blocks Testing
| Issue | Severity | Impact | Fix Time |
|-------|----------|--------|----------|
| LessonCreateDto missing Language | 🔴 Critical | Cannot create lessons | 5 min |
| SessionCreateDto wrong types | 🔴 Critical | Type compilation error | 5 min |
| TypingView.Backspace bug | 🟠 High | Runtime crash on backspace | 5 min |
| LessonRepository doesn't set Language | 🔴 Critical | DB constraint violation | 5 min |
| PerKeyErrors type mismatch | 🔴 Critical | Compilation error | 5 min |

---

## Issue Summary by Category

### 🔴 Critical Issues (Must Fix Before Testing)

**1. Domain Type Mismatch - Language Field**
- File: `src/Shared/Domain.fs` (LessonCreateDto)
- Problem: Missing `Language` field in DTO but required in database
- Fix: Add `Language: Language` to LessonCreateDto type
- Time: 2 min

**2. SessionCreateDto Wrong Types**
- File: `src/Shared/Domain.fs` (SessionCreateDto)
- Problem: Uses `float` for WPM/CPM (should be `int`), includes StartedAt/EndedAt (should be server-generated)
- Fix: Change `Wpm: int`, `Cpm: int`, remove DateTime fields
- Time: 3 min

**3. LessonRepository.createLesson Missing Language Parameter**
- File: `src/Server/Database/LessonRepository.fs` (line ~105)
- Problem: INSERT statement doesn't bind the Language parameter
- Fix: Add `language = dto.Language |> string` to SQL parameters
- Time: 3 min

**4. LessonRepository.updateLesson Missing Language Parameter**
- File: `src/Server/Database/LessonRepository.fs` (line ~145)
- Problem: UPDATE statement doesn't include Language column
- Fix: Add `language = @language` to UPDATE clause and parameters
- Time: 3 min

**5. PerKeyErrors Type Mismatch**
- File: `src/Shared/Domain.fs` (SessionCreateDto vs TypingView)
- Problem: TypingView uses `Map<int, int>` but DTO expects `Map<string, int>`
- Fix: Change SessionCreateDto to `Map<int, int>`
- Time: 2 min

**Subtotal Critical**: ~15 minutes

---

### 🟠 High Priority Issues

**1. TypingView.Backspace Logic Bug**
- File: `src/Client/Pages/TypingView.fs` (line 109)
- Problem: String slicing `.[0..n-2]` fails when index is 0
- Fix: Add bounds check before slicing
- Time: 3 min

**2. LessonHandler.validateLessonCreateDto Missing Language**
- File: `src/Server/Handlers/LessonHandler.fs` (line 220)
- Problem: Validation doesn't check Language field (after adding it to DTO)
- Fix: Add validation for Language field
- Time: 2 min

**3. SessionHandler.validateSessionCreateDto**
- File: `src/Server/Handlers/SessionHandler.fs` (line 95)
- Problem: Incomplete validation logic (will be simpler after removing DateTime)
- Fix: Keep as-is, validation for lessonId existence should stay
- Time: 1 min

**Subtotal High**: ~6 minutes

---

### 🟡 Medium Priority Issues

**1. Hard-Coded Migration File Paths**
- File: `src/Server/Database/DbContext.fs` (line ~35)
- Problem: Relative paths fail if server runs from different directory
- Fix: Use `AppContext.BaseDirectory` to build absolute paths
- Time: 3 min

**2. TypingView Metrics Type Calculation**
- File: `src/Client/Pages/TypingView.fs` (line 36)
- Problem: Calculates WPM/CPM as `int` but should match corrected DTO type
- Fix: Keep as-is after fixing DTO (int is correct)
- Time: 1 min (verification only)

**Subtotal Medium**: ~4 minutes

---

### 🔵 Low Priority Issues

**1. Missing CORS Configuration**
- File: `src/Server/Program.fs` (line ~55)
- Problem: Frontend on different port will get CORS errors
- Fix: Add `use_cors "default"` configuration
- Time: 3 min

**2. Exception Messages Exposed**
- Files: Multiple handlers
- Problem: Exception details exposed in error responses (security)
- Fix: Use generic messages, log exceptions separately
- Time: 5 min (nice-to-have)

**Subtotal Low**: ~8 minutes

---

## Test Scenarios (After Fixes)

### ✅ Happy Path Tests
1. Create lesson with all fields
2. Retrieve lesson by ID
3. List all lessons
4. Update lesson
5. Delete lesson
6. Complete typing session
7. View metrics

### ✅ Error Path Tests
1. POST /api/lessons with missing Language → 400 Bad Request
2. GET /api/lessons/invalid-id → 404 Not Found
3. POST /api/sessions with invalid lessonId → 400 Bad Request
4. Submit session with 0% accuracy → 201 Created (valid)

### ✅ Edge Cases
1. Lesson with max length content (5000 chars)
2. Typing with 0 errors
3. Typing with 100% accuracy
4. Backspace at position 1 (end of first char)
5. Complete typing exactly to end

---

## Fix Checklist

### Phase 1: Domain Types (10 min)
- [ ] Add `Language: Language` to LessonCreateDto
- [ ] Change SessionCreateDto.Wpm from float to int
- [ ] Change SessionCreateDto.Cpm from float to int
- [ ] Remove SessionCreateDto.StartedAt
- [ ] Remove SessionCreateDto.EndedAt
- [ ] Change PerKeyErrors from `Map<string, int>` to `Map<int, int>`

### Phase 2: Repositories (8 min)
- [ ] Fix LessonRepository.createLesson to add Language parameter
- [ ] Fix LessonRepository.updateLesson to add Language parameter
- [ ] Test both functions compile

### Phase 3: Frontend (5 min)
- [ ] Fix TypingView.SubmitSession to remove DateTime mapping
- [ ] Fix TypingView.Backspace bounds check
- [ ] Fix TypingView calculateMetrics return type (should match int for WPM/CPM)

### Phase 4: Handlers & Config (8 min)
- [ ] Add Language validation to LessonHandler
- [ ] Add CORS configuration to Program.fs
- [ ] Fix migration file paths in DbContext.fs

### Phase 5: Verification (5 min)
- [ ] `dotnet build` src/Server/KeyboardTrainer.Server.fsproj
- [ ] `dotnet build` src/Client/KeyboardTrainer.Client.fsproj
- [ ] No compilation errors
- [ ] No type warnings

**Total Est. Time**: ~40 minutes

---

## Commands for Testing (After Fixes)

### Start Database (PostgreSQL required)
```bash
# Option 1: Docker
docker run -e POSTGRES_PASSWORD=trainer123 -p 5434:5434 postgres:15

# Option 2: Local PostgreSQL (if installed)
psql -U trainer -d keyboardtrainer
```

### Start Server
```bash
cd src/Server
dotnet run
# Should output:
# [DB] Running migrations...
# [DB] ✓ Migration 001 completed
# [DB] ✓ Migration 002 (seed data) completed
# Starting Keyboard Trainer server on port 5000...
# Database is ready
```

### Test API Endpoints
```bash
# Get all lessons
curl http://localhost:5000/api/lessons

# Get single lesson (replace GUID)
curl http://localhost:5000/api/lessons/550e8400-e29b-41d4-a716-446655440000

# Create lesson
curl -X POST http://localhost:5000/api/lessons \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Lesson",
    "difficulty": "A1",
    "contentType": "Words",
    "language": "French",
    "textContent": "bonjour monde"
  }'

# Health check
curl http://localhost:5000/health
```

---

## Issues by File

| File | Issues | Severity |
|------|--------|----------|
| src/Shared/Domain.fs | 3 | 🔴 Critical |
| src/Server/Database/LessonRepository.fs | 2 | 🔴 Critical |
| src/Client/Pages/TypingView.fs | 2 | 🟠 High |
| src/Server/Database/DbContext.fs | 1 | 🟡 Medium |
| src/Server/Handlers/LessonHandler.fs | 1 | 🟠 High |
| src/Server/Program.fs | 1 | 🔵 Low |

---

## Risk Assessment

**Compilation Risk**: 🔴 HIGH (5 critical type errors will prevent build)  
**Runtime Risk**: 🟠 MEDIUM (Backspace bug will crash during typing)  
**Database Risk**: 🟠 MEDIUM (Language constraint violation on insert)  
**API Risk**: 🟡 LOW (Endpoints structure is sound, just type issues)

---

## Recommendation

### ⛔ DO NOT PROCEED TO LOCAL TESTING UNTIL:
1. All 5 critical issues are fixed
2. Compilation succeeds without errors
3. Code review verification passes

### ✅ AFTER FIXES:
1. Start PostgreSQL instance
2. Run server with migrations
3. Execute test scenarios from TEST_REVIEW.md
4. Verify all HTTP status codes match expectations

---

## Full Details

For detailed issue descriptions, code examples, and fix instructions, see: [TEST_REVIEW.md](TEST_REVIEW.md)

---

**Reviewed by**: QA Tester  
**Date**: January 25, 2026  
**Status**: ⛔ BLOCKED - Fix critical issues first
