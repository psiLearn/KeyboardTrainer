namespace KeyboardTrainer.Server

module LessonHandler =
    let toLessonDto = LessonHandlerCommon.toLessonDto
    let validateLessonCreateDto = LessonHandlerCommon.validateLessonCreateDto
    let apiError = LessonHandlerCommon.apiError

    let getAllLessons = LessonHandlerCrud.getAllLessons
    let getLessonById = LessonHandlerCrud.getLessonById
    let postLesson = LessonHandlerCrud.postLesson
    let putLesson = LessonHandlerCrud.putLesson
    let deleteLesson = LessonHandlerCrud.deleteLesson

    let getLessonExercise = LessonHandlerExercise.getLessonExercise
    let postProbabilityExercise = LessonHandlerExercise.postProbabilityExercise
