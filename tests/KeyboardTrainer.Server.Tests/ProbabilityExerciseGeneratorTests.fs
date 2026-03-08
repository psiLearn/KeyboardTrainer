namespace KeyboardTrainer.Server.Tests

open Xunit
open KeyboardTrainer.Server

module ProbabilityExerciseGeneratorTests =
    [<Fact>]
    let ``generateFromProbabilityJson returns generated text for valid payload`` () =
        let payload = """{"kind":"unigram-v1","generatedLength":12,"wordLength":4,"weights":{"q":0.5,"w":0.5}}"""

        let result = ProbabilityExerciseGenerator.generateFromProbabilityJson payload None None

        match result with
        | Ok content ->
            Assert.False(System.String.IsNullOrWhiteSpace content)
            Assert.Equal(14, content.Length)
        | Error message ->
            Assert.True(false, sprintf "Expected Ok but got Error: %s" message)

    [<Fact>]
    let ``generateFromProbabilityJson rejects invalid multi-character weight keys`` () =
        let payload = """{"kind":"unigram-v1","generatedLength":8,"wordLength":4,"weights":{"ab":1.0}}"""

        let result = ProbabilityExerciseGenerator.generateFromProbabilityJson payload None None

        match result with
        | Ok _ -> Assert.True(false, "Expected error for invalid weight key")
        | Error message -> Assert.Contains("Invalid weight key", message)

    [<Fact>]
    let ``generateFromProbabilityJson accepts space alias key`` () =
        let payload = """{"kind":"unigram-v1","generatedLength":8,"wordLength":4,"weights":{"space":1.0}}"""

        let result = ProbabilityExerciseGenerator.generateFromProbabilityJson payload None None

        match result with
        | Ok content -> Assert.Equal("         ", content)
        | Error message -> Assert.True(false, sprintf "Expected Ok but got Error: %s" message)

    [<Fact>]
    let ``generateFromProbabilityJson accepts literal space key`` () =
        let payload = """{"kind":"unigram-v1","generatedLength":8,"wordLength":4,"weights":{" ":1.0}}"""

        let result = ProbabilityExerciseGenerator.generateFromProbabilityJson payload None None

        match result with
        | Ok content -> Assert.Equal("         ", content)
        | Error message -> Assert.True(false, sprintf "Expected Ok but got Error: %s" message)
