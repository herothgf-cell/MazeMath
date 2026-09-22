using System;
using MazeMath.Maze.Data;
using MazeMath.Maze.Validation;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGenerationCoordinator
    {
        private const int MaxAttempts = 10;

        private readonly IMazeGenerator _generator;
        private readonly IMazeValidator _validator;
        private readonly MazeSeedService _seedService;
        private readonly SafeMazeFactory _safeMazeFactory;

        public MazeGenerationCoordinator(
            IMazeGenerator generator,
            IMazeValidator validator,
            MazeSeedService seedService,
            SafeMazeFactory safeMazeFactory)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _seedService = seedService ?? throw new ArgumentNullException(nameof(seedService));
            _safeMazeFactory = safeMazeFactory ?? throw new ArgumentNullException(nameof(safeMazeFactory));
        }

        public MazeGenerationResult GenerateValidated(
            string profileId,
            string chapterId,
            int baseSeedOffset,
            MazeGenerationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            uint lastSeed = 0u;

            for (var attemptIndex = 0; attemptIndex < MaxAttempts; attemptIndex++)
            {
                lastSeed = _seedService.CreateSeed(
                    profileId,
                    chapterId,
                    attemptIndex,
                    baseSeedOffset);

                var generated = _generator.Generate(
                    new MazeGenerationRequest(chapterId, lastSeed, settings));

                var validation = _validator.Validate(generated.Graph, settings);
                if (validation.IsValid)
                {
                    return new MazeGenerationResult(
                        generated.Graph,
                        lastSeed,
                        usedSafeLayout: false,
                        attemptCount: attemptIndex + 1);
                }
            }

            return new MazeGenerationResult(
                _safeMazeFactory.CreateChapter1SafeLayout(),
                lastSeed,
                usedSafeLayout: true,
                attemptCount: MaxAttempts);
        }
    }
}
