# PHASE 2 TASK BACKLOG: Client HTTP & UI Implementation

**Estimated Duration**: 8-12 days  
**Priority**: HIGH (Required before production deployment)  
**Depends On**: Phase 1 completion ✅  
**Blockers**: None

---

## Task 2.1: Modernize ApiClient HTTP Integration (3 days)

### Description
Replace deprecated Fable.Fetch API with modern Fable.SimpleHttp or Fable.Http client. Current ApiClient uses `RequestProperties` enum pattern which is outdated.

### Current Issues
```
ERROR FS0001: This expression should have type Async<'a>
              It is however of type JS.Promise<Response>
              
ERROR FS0039: The type 'HttpMethod' does not define field 'Get'
              (Should use HttpMethod.GET)
              
ERROR FS0072: Lookup on object of indeterminate type
              Missing type annotations for Fetch operations
```

### Implementation Approach

**Option A: Fable.SimpleHttp** (Recommended for MVP)
```fsharp
// Lightweight, no-dependency HTTP client
// Pros: Simple, small bundle size, perfect for basic REST
// Cons: Less feature-rich

open Fable.SimpleHttp

let getAllLessons () : Async<Result<LessonDto list, string>> =
    async {
        try
            let! (status, body) = Http.get "/api/lessons"
            match status with
            | 200 ->
                let lessons = Json.parseAs<LessonDto list> body
                return Ok lessons
            | 404 -> return Error "Lessons not found"
            | status -> return Error $"Server error: {status}"
        with ex ->
            return Error ex.Message
    }

let createSession (dto: SessionCreateDto) : Async<Result<SessionDto, string>> =
    async {
        try
            let payload = Json.stringify dto
            let! (status, body) = Http.post "/api/sessions" payload
            match status with
            | 201 ->
                let session = Json.parseAs<SessionDto> body
                return Ok session
            | 400 ->
                let error = Json.parseAs<ApiError> body
                return Error error.Message
            | status -> return Error $"Server error: {status}"
        with ex ->
            return Error ex.Message
    }
```

**Option B: Fable.Http**
```fsharp
// More feature-rich HTTP client
// Pros: Better error handling, request customization
// Cons: Larger bundle, more complex

open Fable.Http

let getAllLessons () : Async<Result<LessonDto list, string>> =
    Http.get<LessonDto list> "/api/lessons"
    |> Async.map (function
        | Ok data -> Ok data
        | Error msg -> Error msg)
```

### Tasks
- [ ] Add Fable.SimpleHttp dependency to .fsproj
- [ ] Rewrite getAllLessons() using new pattern
- [ ] Rewrite getLessonById() 
- [ ] Rewrite createLesson()
- [ ] Rewrite updateLesson()
- [ ] Rewrite deleteLesson()
- [ ] Rewrite createSession()
- [ ] Rewrite getSessionsByLesson()
- [ ] Update error handling to parse ApiError from server
- [ ] Test all endpoints against running server
- [ ] Update response type annotations

### Acceptance Criteria
- [x] Client compiles with 0 errors
- [x] All HTTP methods (GET, POST, PUT, DELETE) work
- [x] Error responses properly deserialized
- [x] Async patterns integrate with Elmish
- [x] Type annotations are explicit

### Testing Strategy
```fsharp
// Test script to verify endpoints
let testApiClient () =
    async {
        // Test health check
        let! health = ApiClient.healthCheck()
        
        // Test get all lessons
        let! lessons = ApiClient.getAllLessons()
        
        // Test create session
        let dto = { LessonId = Guid.NewGuid(); Wpm = 60; ... }
        let! session = ApiClient.createSession dto
        
        // Verify responses
        match session with
        | Ok s -> printfn "Session created: %A" s.Id
        | Error e -> printfn "Error: %s" e
    }
```

---

## Task 2.2: Fix Elmish Cmd API Calls (1 day)

### Description
Update all `Cmd.OfAsync.perform` calls to use correct API based on Fable.Elmish version.

### Current Status
- Partially fixed in previous attempt
- Need to verify correct API after ApiClient is updated
- Might be `Cmd.ofAsync.perform` or `Cmd.OfAsync.perform` depending on version

### Tasks
- [ ] Run `dotnet add package Fable.Elmish --version` to check version
- [ ] Review Fable.Elmish documentation for Cmd API
- [ ] Fix StartScreen.fs: `Cmd.OfAsync.perform ApiClient.getAllLessons()`
- [ ] Fix Metrics.fs: Two `Cmd.OfAsync.perform` calls for sessions
- [ ] Fix TypingView.fs: `Cmd.OfAsync.perform ApiClient.createSession()`
- [ ] Test that Pages compile after ApiClient fix

### Acceptance Criteria
- [x] All client files compile with 0 errors
- [x] Cmd calls match Fable.Elmish API exactly
- [x] Pages can be imported in App.fs without errors

---

## Task 2.3: Add Error Boundary & User Feedback (2 days)

### Description
Implement proper error handling UI to display validation errors, network errors, and server responses gracefully.

### Error Types to Handle
```fsharp
// Server validation error
type ValidationError = {
    Field: string
    Message: string
}

type ApiError = {
    Message: string
    StatusCode: int
    Errors: ValidationError list option
}

// Client network error
type NetworkError =
    | Timeout
    | Offline
    | ConnectionRefused
    | HttpError of int * string

// Combined error type
type AppError =
    | Validation of ValidationError list
    | Server of string
    | Network of NetworkError
    | Unknown of string
```

### UI Components Needed

**ErrorAlert Component**
```fsharp
module ErrorAlert =
    let view error dispatch =
        div [ ClassName "alert alert-danger" ] [
            button [ 
                ClassName "close"
                OnClick (fun _ -> dispatch ClearError)
            ] [ str "×" ]
            
            match error with
            | Validation errors ->
                ul [] [
                    for err in errors do
                        li [] [ str $"{err.Field}: {err.Message}" ]
                ]
            | Server msg ->
                p [] [ str msg ]
            | Network err ->
                p [] [ str (describeNetworkError err) ]
            | Unknown msg ->
                p [] [ str msg ]
        ]
```

**LoadingSpinner Component**
```fsharp
module LoadingSpinner =
    let view =
        div [ ClassName "spinner" ] [
            div [ ClassName "spinner-border" ] []
            p [] [ str "Loading..." ]
        ]
```

**Pages Integration**
- StartScreen: Show error if lesson load fails
- TypingView: Show submission errors
- Metrics: Show error if session load fails

### Tasks
- [ ] Create ErrorAlert component
- [ ] Create LoadingSpinner component  
- [ ] Add Error and Loading states to each page Model
- [ ] Add UI rendering for error states
- [ ] Implement error recovery actions (Retry button)
- [ ] Add CSS styling for alerts and spinners

### Acceptance Criteria
- [x] Validation errors displayed with field names
- [x] Network errors show user-friendly messages
- [x] Loading state visible during async operations
- [x] Error clearing/dismissal works

---

## Task 2.4: Add Session Persistence (2 days)

### Description
Store typing sessions locally in browser, sync with server, handle offline scenarios.

### Local Storage Strategy
```fsharp
// Store recent sessions locally
type LocalSession = {
    Id: Guid
    LessonId: Guid
    Wpm: int
    Cpm: int
    Accuracy: double
    ErrorCount: int
    CreatedAt: DateTime
    SyncedWithServer: bool
}

// Key: "keyboard-trainer-sessions"
// Value: LocalSession list (JSON serialized)
```

### Sync Logic
```fsharp
// On app startup:
1. Load sessions from LocalStorage
2. Filter for unsyncedWithServer = true
3. For each unsynced session:
   - Attempt POST /api/sessions
   - If success: Set syncedWithServer = true
   - If failure: Keep local, retry later
4. Load server sessions for display

// On new session submit:
1. Store in LocalStorage with syncedWithServer = false
2. Attempt POST to server
3. On success: Mark as synced
4. On failure: Retry in background
```

### Tasks
- [ ] Add Fable.Browser.LocalStorage package
- [ ] Create LocalStorage module for session persistence
- [ ] Add sync logic to App.fs on startup
- [ ] Add background sync after successful typing session
- [ ] Implement retry mechanism (exponential backoff)
- [ ] Show sync status indicator in UI
- [ ] Add clear local data option in Metrics view

### Acceptance Criteria
- [x] Sessions persist across browser refresh
- [x] Background sync happens transparently
- [x] Offline sessions sync when connection restored
- [x] User can see sync status

---

## Task 2.5: UI Polish & Styling (3 days)

### Description
Implement responsive design, animations, and improved UX for typing experience.

### Character-by-Character Rendering

**Current Implementation** (TypingView.fs):
```fsharp
for i in 0 .. model.Lesson.Content.Length - 1 do
    let char = model.Lesson.Content.[i]
    let className = 
        if i < model.CurrentCharIndex then
            if Map.containsKey i model.Errors then
                "char char-error"
            else
                "char char-correct"
        elif i = model.CurrentCharIndex then
            "char char-current"
        else
            "char char-next"
    
    span [ ClassName className ] [ str (string char) ]
```

**Enhancement Needed**:
- [ ] Add CSS transitions for character highlighting
- [ ] Implement blinking cursor
- [ ] Add text selection prevention
- [ ] Handle long lessons with scrolling
- [ ] Keyboard shortcuts (Start, Stop, Reset)

### Real-Time Metrics Display

```fsharp
div [ ClassName "metrics-live" ] [
    div [ ClassName "metric" ] [
        span [ ClassName "label" ] [ str "WPM" ]
        span [ ClassName "value" ] [ str (string model.Wpm) ]
    ]
    div [ ClassName "metric" ] [
        span [ ClassName "label" ] [ str "CPM" ]
        span [ ClassName "value" ] [ str (string model.Cpm) ]
    ]
    div [ ClassName "metric" ] [
        span [ ClassName "label" ] [ str "Accuracy" ]
        span [ ClassName "value" ] [ str $"{model.Accuracy}%" ]
    ]
]
```

### Progress & Animations
- [ ] Progress bar fills as user types
- [ ] Confetti animation on completion
- [ ] Smooth transitions between pages
- [ ] Error character red flash
- [ ] Success character green highlight

### Responsive Design
- [ ] Mobile-friendly layout
- [ ] Touch keyboard optimizations
- [ ] Tablet text size adjustments
- [ ] Landscape/portrait handling

### CSS Organization
```
styles/
├── main.css           (Core layout)
├── typography.css     (Fonts, text styling)
├── components.css     (Buttons, cards, alerts)
├── typing.css         (Character highlighting, metrics)
├── animations.css     (Transitions, effects)
└── responsive.css     (Media queries)
```

### Tasks
- [ ] Create CSS structure above
- [ ] Implement character styling classes
- [ ] Add cursor animation
- [ ] Create metrics display component
- [ ] Add progress bar styling
- [ ] Implement page transitions
- [ ] Add responsive breakpoints
- [ ] Test on mobile devices
- [ ] Optimize font loading

### Acceptance Criteria
- [x] Typing experience is smooth (60 FPS)
- [x] Mobile layout is readable and usable
- [x] Metrics display in real-time
- [x] Character highlighting is clear
- [x] No layout shifts during interaction

---

## Task 2.6: Integration Testing (2 days)

### Description
Test full client-server integration with real server running.

### Test Scenarios

**Lesson Flow**
```
1. Load lessons from API
2. Select a lesson
3. Navigate to typing view
4. Verify lesson content displays
5. Type some characters
6. Verify character highlighting works
7. Submit session
8. Verify metrics calculated
9. Check session appears in stats
```

**Error Scenarios**
```
1. API down → Show error message
2. Network timeout → Show retry
3. Invalid input → Show validation errors
4. Server 500 → Show error, option to retry
```

**Offline Scenario**
```
1. Disable network
2. Submit session
3. Show local storage message
4. Re-enable network
5. Verify auto-sync
```

### Tasks
- [ ] Set up test server instance
- [ ] Create E2E test script using Playwright or Cypress
- [ ] Test all happy paths
- [ ] Test error scenarios
- [ ] Test offline/online transitions
- [ ] Performance testing (time to first byte, metrics calculation)
- [ ] Load testing (concurrent users)

### Acceptance Criteria
- [x] All user flows complete successfully
- [x] Error messages are appropriate
- [x] No data loss in offline scenarios
- [x] Page load < 2 seconds
- [x] Metrics calculation < 50ms

---

## Task 2.7: Documentation & Deployment (1 day)

### Description
Update documentation, prepare for production deployment.

### Documentation Updates
- [ ] Update README.md with client setup instructions
- [ ] Document API contract in OpenAPI/Swagger format
- [ ] Create user guide with screenshots
- [ ] Document deployment process
- [ ] Add troubleshooting guide

### Deployment Checklist
- [ ] Set up staging environment
- [ ] Configure production database
- [ ] Set up SSL certificates
- [ ] Configure CORS for production domain
- [ ] Set up monitoring/alerts
- [ ] Create backup strategy
- [ ] Document rollback procedure

### Tasks
- [ ] Write client-server integration documentation
- [ ] Create OpenAPI spec for API
- [ ] Prepare Docker Compose for full stack
- [ ] Write deployment guide
- [ ] Create runbook for operations team
- [ ] Set up health monitoring

---

## Implementation Priority

### Critical Path (Must Complete)
1. **Task 2.1**: Modernize HTTP client (blocks everything else)
2. **Task 2.2**: Fix Elmish Cmd (depends on 2.1)
3. **Task 2.3**: Error handling (improves user experience)
4. **Task 2.6**: Integration testing (validates everything)

### High Priority (Should Complete)
5. **Task 2.5**: UI Polish (production quality)
6. **Task 2.4**: Session persistence (offline support)

### Desirable (Nice to Have)
7. **Task 2.7**: Documentation (ongoing process)

### Timeline
```
Week 1:
  Mon-Tue: Task 2.1 (ApiClient modernization)
  Wed:     Task 2.2 (Elmish Cmd fixes)
  Thu:     Task 2.3 (Error handling)
  Fri:     Task 2.6 (Testing)

Week 2:
  Mon-Tue: Task 2.5 (UI Polish)
  Wed-Thu: Task 2.4 (Session persistence)
  Fri:     Task 2.7 (Documentation & deployment)
  
Plus: 2-3 days buffer for issues & reviews
```

---

## Success Criteria

**Definition of Done** (Phase 2 Complete):
- [x] Client builds with 0 errors
- [x] All API endpoints working end-to-end
- [x] Error handling covers all scenarios
- [x] UI responsive and polished
- [x] Integration tests pass
- [x] Documentation complete
- [x] Deployed to staging
- [x] User acceptance testing passed

---

## Risk Register

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Fable API changes | High | Medium | Pin package versions, test early |
| Performance issues | Medium | Low | Profiling, lazy loading, code splitting |
| Browser compatibility | Medium | Low | BrowserStack testing, polyfills |
| Offline sync conflicts | High | Low | Version tracking, conflict resolution |
| User experience complexity | Medium | Medium | User testing, iterate on UX |

---

## Dependencies

**Packages to Add**:
```xml
<PackageReference Include="Fable.SimpleHttp" Version="3.4.0" />
<!-- or -->
<PackageReference Include="Fable.Http" Version="X.X.X" />

<PackageReference Include="Fable.Browser.LocalStorage" Version="..." />
<!-- Optional for animations: -->
<PackageReference Include="Fable.AnimationFrameLoop" Version="..." />
```

**External Services**:
- Running KeyboardTrainer.Server instance
- PostgreSQL database
- (Optional) Error tracking service

---

## References

- Fable Documentation: https://fable.io/
- Elmish Guide: https://github.com/elmish/elmish/wiki
- SAFE Stack: https://safe-stack.github.io/
- F# Async Documentation: https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/asynchronous-programming-with-async

---

**End of Phase 2 Backlog**

*Next Review: After Phase 1 deployment to staging*
