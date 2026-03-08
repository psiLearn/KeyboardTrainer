namespace KeyboardTrainer.LectureCreator

open KeyboardTrainer.Shared

type Input = {
    Title: string
    Difficulty: Difficulty
    ContentType: ContentType
    Language: Language
    Content: string
}
