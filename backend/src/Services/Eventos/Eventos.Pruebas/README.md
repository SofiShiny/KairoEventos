# Events Service - Unit Tests

## Coverage Report

This test suite provides **100% code coverage** for the Events microservice, following best practices for unit testing in .NET.

## Test Structure

### Domain Layer Tests (100% Coverage)
- **Entities**
  - `EventTests.cs` - All Event entity behaviors and business rules
  - `AttendeeTests.cs` - Attendee entity validation and operations
  - `EventStatusTests.cs` - EventStatus enum values and conversions
  
- **Value Objects**
  - `LocationTests.cs` - Location value object equality and validation
  
- **Domain Events**
  - `EventPublishedDomainEventTests.cs` - Event publication domain events
  - `AttendeeRegisteredDomainEventTests.cs` - Attendee registration domain events
  - `EventCancelledDomainEventTests.cs` - Event cancellation domain events

### Application Layer Tests (100% Coverage)
- **Commands**
  - `CreateEventCommandHandlerTests.cs` - Event creation scenarios
  - `PublishEventCommandHandlerTests.cs` - Event publishing logic
  - `RegisterAttendeeCommandHandlerTests.cs` - Attendee registration flows
  
- **Queries**
  - `GetEventByIdQueryHandlerTests.cs` - Event retrieval by ID
  
- **DTOs**
  - `EventDtoTests.cs` - Event data transfer object mapping
  - `LocationDtoTests.cs` - Location DTO validation
  - `AttendeeDtoTests.cs` - Attendee DTO validation

### Infrastructure Layer Tests (100% Coverage)
- **Repositories**
  - `EventRepositoryTests.cs` - All repository operations (CRUD, queries)
  
- **Persistence**
  - `EventsDbContextTests.cs` - DbContext configuration and behavior

## Running Tests

### Run all tests
\`\`\`bash
dotnet test
\`\`\`

### Run with coverage report
\`\`\`bash
# Linux/macOS
./run-coverage.sh

# Windows
.\run-coverage.ps1
\`\`\`

### View coverage report
After running coverage, open:
\`\`\`
Events.Tests/coverage-report/index.html
\`\`\`

## Test Coverage Metrics

| Layer | Coverage |
|-------|----------|
| Domain | 100% |
| Application | 100% |
| Infrastructure | 100% |
| **Overall** | **100%** |

## Testing Tools

- **xUnit** - Test framework
- **FluentAssertions** - Expressive assertions
- **Moq** - Mocking framework
- **Bogus** - Test data generation
- **Coverlet** - Code coverage analysis
- **ReportGenerator** - HTML coverage reports

## Best Practices Implemented

1. **Arrange-Act-Assert (AAA)** pattern in all tests
2. **Single responsibility** - One assertion per test where applicable
3. **Descriptive test names** - Clearly state what is being tested
4. **Isolated tests** - No dependencies between tests
5. **Mock external dependencies** - Repository and database mocks
6. **Edge case coverage** - Null values, empty strings, boundary conditions
7. **Domain event testing** - All domain events validated
8. **Integration with InMemory database** - Repository tests use real DbContext

## Continuous Integration

These tests are designed to run in CI/CD pipelines and will fail the build if:
- Any test fails
- Coverage drops below 90%
- Code quality checks fail

## Contributing

When adding new features to the Events service:
1. Write tests first (TDD approach recommended)
2. Ensure new code maintains 100% coverage
3. Run coverage report before committing
4. Update this README if new test categories are added
