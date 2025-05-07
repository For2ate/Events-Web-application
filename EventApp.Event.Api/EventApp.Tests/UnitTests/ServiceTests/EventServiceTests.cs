using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using EventApp.Api.Core.Services;
using EventApp.Api.Exceptions;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;


namespace EventApp.Tests.UnitTests.ServiceTests {

    public class EventServiceTests {

        private readonly IFixture _fixture;
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<IEventCategoryRepository> _mockEventCategoryRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<EventService>> _mockLogger;
        private readonly EventService _sut;

        public EventServiceTests() {

            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new OmitOnRecursionBehaviorCustomization());

            _mockEventRepository = _fixture.Freeze<Mock<IEventRepository>>();
            _mockEventCategoryRepository = _fixture.Freeze<Mock<IEventCategoryRepository>>();
            _mockMapper = _fixture.Freeze<Mock<IMapper>>();
            _mockLogger = _fixture.Freeze<Mock<ILogger<EventService>>>();

            _sut = _fixture.Create<EventService>();

        }

        private class OmitOnRecursionBehaviorCustomization : ICustomization {
            public void Customize(IFixture fixture) {
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }
        }

        // =======================================================================
        // Тесты для  GetEventByIdAsync
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task GetEventByIdAsync_WhenEventExists_ShouldReturnMappedEvent(Guid eventId) {
            // Arrange
            var eventEntity = _fixture.Build<EventEntity>()
                .With(e => e.Id, eventId)
                .Without(e => e.Category)
                .Without(e => e.Registrations)
                .Create();

            var expectedResponse = _fixture.Create<EventFullResponseModel>();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(eventEntity);

            _mockMapper.Setup(mapper => mapper.Map<EventFullResponseModel>(eventEntity))
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetEventByIdAsync(eventId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockEventRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(eventEntity), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetEventByIdAsync_WhenEventDoesNotExist_ShouldReturnNull(Guid eventId) {

            // Arrange
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync((EventEntity?)null);

            // Act
            var result = await _sut.GetEventByIdAsync(eventId);

            // Assert
            result.Should().BeNull();
            _mockEventRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()), Times.Never);

        }

        [Theory]
        [AutoData]
        public async Task GetEventByIdAsync_WhenRepositoryThrowsException_ShouldLogAndRethrow(Guid eventId) {

            // Arrange
            var exceptionMessage = "Database connection failed";
            var repositoryException = new InvalidOperationException(exceptionMessage);

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ThrowsAsync(repositoryException);

            // Act
            Func<Task> act = async () => await _sut.GetEventByIdAsync(eventId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(exceptionMessage);

            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains($"Error while get event {eventId}") &&
                        v.ToString().Contains(eventId.ToString())),
                    repositoryException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

        }

        // =======================================================================
        // Тесты для GetAllEventsAsync
        // =======================================================================

        [Fact]
        public async Task GetAllEventsAsync_WhenEventsExist_ShouldReturnMappedEvents() {
            // Arrange
            var eventEntities = _fixture.CreateMany<EventEntity>(3).ToList();
            var expectedResponseDtos = _fixture.CreateMany<EventFullResponseModel>(3).ToList();

            _mockEventRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(eventEntities);

            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<EventFullResponseModel>>(eventEntities))
                .Returns(expectedResponseDtos);

            // Act
            var result = await _sut.GetAllEventsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponseDtos);
            _mockEventRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<IEnumerable<EventFullResponseModel>>(eventEntities), Times.Once);
        }

        [Fact]
        public async Task GetAllEventsAsync_WhenNoEventsExist_ShouldReturnEmptyMappedList() {
            // Arrange
            var emptyEventEntities = new List<EventEntity>();
            var emptyExpectedResponseDtos = new List<EventFullResponseModel>();

            _mockEventRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(emptyEventEntities);

            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<EventFullResponseModel>>(emptyEventEntities))
                .Returns(emptyExpectedResponseDtos);

            // Act
            var result = await _sut.GetAllEventsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockEventRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<IEnumerable<EventFullResponseModel>>(emptyEventEntities), Times.Once);
        }

        [Fact]
        public async Task GetAllEventsAsync_WhenRepositoryThrowsException_ShouldLogAndRethrow() {

            // Arrange
            var exceptionMessage = "Failed to retrieve all events from database";
            var repositoryException = new InvalidOperationException(exceptionMessage);

            _mockEventRepository.Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(repositoryException);

            // Act
            Func<Task> act = async () => await _sut.GetAllEventsAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage(exceptionMessage);

        }

        // =======================================================================
        // Тесты для GetEventByNameAsync
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task GetEventByNameAsync_WhenEventExists_ShouldReturnMappedEvent(string eventName) {

            // Arrange
            var eventEntity = _fixture.Build<EventEntity>()
                                      .With(e => e.Name, eventName)
                                      .Create();

            var expectedResponse = _fixture.Create<EventFullResponseModel>();

            _mockEventRepository.Setup(repo => repo.GetEventByNameAsync(eventName))
                .ReturnsAsync(eventEntity);
            _mockMapper.Setup(mapper => mapper.Map<EventFullResponseModel>(eventEntity))
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetEventByNameAsync(eventName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockEventRepository.Verify(repo => repo.GetEventByNameAsync(eventName), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(eventEntity), Times.Once);

        }

        [Theory]
        [AutoData]
        public async Task GetEventByNameAsync_WhenEventDoesNotExist_ShouldThrowNotFoundException(string eventName) {

            // Arrange
            _mockEventRepository.Setup(repo => repo.GetEventByNameAsync(eventName))
                .ReturnsAsync((EventEntity?)null);

            // Act
            Func<Task> act = async () => await _sut.GetEventByNameAsync(eventName);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Resource 'event' with identifier '{eventName}' not found.");

            _mockEventRepository.Verify(repo => repo.GetEventByNameAsync(eventName), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()), Times.Never);
        }

        [Theory]
        [AutoData]
        public async Task GetEventByNameAsync_WhenRepositoryThrowsOtherException_ShouldRethrow(string eventName) {

            // Arrange
            var repositoryException = new InvalidOperationException("Database access error");
            _mockEventRepository.Setup(repo => repo.GetEventByNameAsync(eventName))
                .ThrowsAsync(repositoryException);

            // Act
            Func<Task> act = async () => await _sut.GetEventByNameAsync(eventName);

            // Assert
            var thrownException = ( await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database access error") ).Which; // 

            thrownException.Should().BeSameAs(repositoryException);

            _mockEventRepository.Verify(repo => repo.GetEventByNameAsync(eventName), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()), Times.Never);

        }

        // =======================================================================
        // Тесты для CreateEventAsync
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task CreateEventAsync_WhenCategoryExistsAndDataIsValid_ShouldCreateAndReturnEvent(
                CreateEventRequestModel requestModel
            ) {

            // Arrange
            var categoryEntity = _fixture.Build<EventCategoryEntity>()
                                         .With(c => c.Id, requestModel.CategoryId)
                                         .Create();

            var eventEntityToCreate = _fixture.Build<EventEntity>()
                                             .OmitAutoProperties()
                                             .With(e => e.Name, requestModel.Name)
                                             .With(e => e.Description, requestModel.Description)
                                             .With(e => e.DateOfEvent, requestModel.DateOfEvent)
                                             .With(e => e.MaxNumberOfParticipants, requestModel.MaxNumberOfParticipants)
                                             .With(e => e.ImageUrl, requestModel.ImageUrl)
                                             .With(e => e.CategoryId, requestModel.CategoryId)
                                             .Create();

            var expectedResponse = _fixture.Create<EventFullResponseModel>();

            _mockEventCategoryRepository.Setup(repo => repo.GetByIdAsync(requestModel.CategoryId))
                .ReturnsAsync(categoryEntity);

            _mockMapper.Setup(mapper => mapper.Map<EventEntity>(requestModel))
                .Returns(eventEntityToCreate);

            _mockEventRepository.Setup(repo => repo.AddAsync(It.Is<EventEntity>(e => e.Id != Guid.Empty && e.Name == requestModel.Name)))
                .Returns(Task.CompletedTask)
                .Callback<EventEntity>(savedEntity => { });

            _mockMapper.Setup(mapper => mapper.Map<EventFullResponseModel>(It.Is<EventEntity>(e => e.Name == requestModel.Name)))
                .Returns(expectedResponse);


            // Act
            var result = await _sut.CreateEventAsync(requestModel);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);

            _mockEventCategoryRepository.Verify(repo => repo.GetByIdAsync(requestModel.CategoryId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventEntity>(requestModel), Times.Once);
            _mockEventRepository.Verify(repo => repo.AddAsync(It.Is<EventEntity>(e => e.Id != Guid.Empty && e.Name == requestModel.Name)), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.Is<EventEntity>(e => e.Id != Guid.Empty && e.Name == requestModel.Name)), Times.Once);

        }

        [Theory]
        [AutoData]
        public async Task CreateEventAsync_WhenCategoryDoesNotExist_ShouldThrowArgumentException(CreateEventRequestModel requestModel) {

            // Arrange
            _mockEventCategoryRepository.Setup(repo => repo.GetByIdAsync(requestModel.CategoryId))
                .ReturnsAsync((EventCategoryEntity?)null); // Категория не найдена

            // Act
            Func<Task> act = async () => await _sut.CreateEventAsync(requestModel);

            // Assert
            var exceptionAssertions = await act.Should().ThrowAsync<ArgumentException>()
                    .WithMessage($"Category with Id {requestModel.CategoryId} not found. (Parameter '{nameof(requestModel.CategoryId)}')");


            exceptionAssertions.And.ParamName.Should().Be(nameof(requestModel.CategoryId));

            _mockEventRepository.Verify(repo => repo.AddAsync(It.IsAny<EventEntity>()), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<EventEntity>(It.IsAny<CreateEventRequestModel>()), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()), Times.Never);

        }

        [Theory]
        [AutoData]
        public async Task CreateEventAsync_WhenRepositoryAddAsyncThrowsException_ShouldRethrow(
    CreateEventRequestModel requestModel) {
            // Arrange
            var categoryEntity = _fixture.Build<EventCategoryEntity>()
                                         .With(c => c.Id, requestModel.CategoryId)
                                         .Create();

            var eventEntityToSave = _fixture.Build<EventEntity>()
                                             .OmitAutoProperties()
                                             .With(e => e.Name, requestModel.Name)
                                             .With(e => e.CategoryId, requestModel.CategoryId)
                                             .Create();

            var repositoryException = new InvalidOperationException("Failed to save event to database");

            _mockEventCategoryRepository.Setup(repo => repo.GetByIdAsync(requestModel.CategoryId))
                .ReturnsAsync(categoryEntity);

            _mockMapper.Setup(mapper => mapper.Map<EventEntity>(requestModel))
                .Returns(eventEntityToSave);

            _mockEventRepository.Setup(repo => repo.AddAsync(It.Is<EventEntity>(e => e.Name == requestModel.Name)))
                .ThrowsAsync(repositoryException);

            // Act
            Func<Task> act = async () => await _sut.CreateEventAsync(requestModel);

            // Assert
            var thrownException = ( await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to save event to database") ).Which;

            thrownException.Should().BeSameAs(repositoryException);

            _mockEventCategoryRepository.Verify(repo => repo.GetByIdAsync(requestModel.CategoryId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventEntity>(requestModel), Times.Once);
            _mockEventRepository.Verify(repo => repo.AddAsync(It.Is<EventEntity>(e => e.Id != Guid.Empty && e.Name == requestModel.Name)), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()), Times.Never);

        }


        // =======================================================================
        // Тесты для UpdateEventAsync
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task UpdateEventAsync_WhenEventAndCategoryExistAndDataIsValid_ShouldUpdateCommitAndReturnMappedEvent(
    UpdateEventRequestModel updateModel) {
            // Arrange
            var existingEventEntity = _fixture.Build<EventEntity>()
                                              .With(e => e.Id, updateModel.Id)
                                              .Create();

            EventCategoryEntity? categoryEntity = null;

            bool shouldCheckCategory = updateModel.CategoryId != Guid.Empty && updateModel.CategoryId != existingEventEntity.CategoryId;
            if (shouldCheckCategory) {
                categoryEntity = _fixture.Build<EventCategoryEntity>()
                                         .With(c => c.Id, updateModel.CategoryId)
                                         .Create();

                _mockEventCategoryRepository.Setup(repo => repo.GetByIdAsync(updateModel.CategoryId))
                    .ReturnsAsync(categoryEntity);

            } else if (updateModel.CategoryId == Guid.Empty) {
                updateModel.CategoryId = existingEventEntity.CategoryId;
            }

            var updatedEntityFromRepoCall = _fixture.Build<EventEntity>()
                                              .With(e => e.Id, updateModel.Id)
                                              .With(e => e.Name, updateModel.Name)
                                              .With(e => e.Description, updateModel.Description)
                                              .With(e => e.DateOfEvent, updateModel.DateOfEvent)
                                              .With(e => e.MaxNumberOfParticipants, updateModel.MaxNumberOfParticipants)
                                              .With(e => e.ImageUrl, updateModel.ImageUrl)
                                              .With(e => e.CategoryId, updateModel.CategoryId)
                                              .Create();

            var expectedResponse = _fixture.Create<EventFullResponseModel>();
            var mockDbContextTransaction = new Mock<IDbContextTransaction>();

            _mockEventRepository.Setup(repo => repo.BeginTransactionAsync())
                .ReturnsAsync(mockDbContextTransaction.Object);

            _mockEventRepository.SetupSequence(repo => repo.GetByIdAsync(updateModel.Id))
                .ReturnsAsync(existingEventEntity)
                .ReturnsAsync(updatedEntityFromRepoCall);

            _mockMapper.Setup(mapper => mapper.Map(updateModel, existingEventEntity))
                .Callback<UpdateEventRequestModel, EventEntity>((sourceModel, destinationEntity) => {
                    destinationEntity.Name = sourceModel.Name;
                    destinationEntity.Description = sourceModel.Description;
                    destinationEntity.DateOfEvent = sourceModel.DateOfEvent;
                    destinationEntity.MaxNumberOfParticipants = sourceModel.MaxNumberOfParticipants;
                    destinationEntity.ImageUrl = sourceModel.ImageUrl;
                    destinationEntity.CategoryId = sourceModel.CategoryId;
                });

            _mockEventRepository.Setup(repo => repo.UpdateAsync(It.Is<EventEntity>(e => e.Id == updateModel.Id)))
                .Returns(Task.CompletedTask);

            mockDbContextTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(mapper => mapper.Map<EventFullResponseModel>(updatedEntityFromRepoCall))
                .Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateEventAsync(updateModel);

            // Assert

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);

            _mockEventRepository.Verify(repo => repo.BeginTransactionAsync(), Times.Once);
            _mockEventRepository.Verify(repo => repo.GetByIdAsync(updateModel.Id), Times.Exactly(2));

            if (shouldCheckCategory && categoryEntity != null) {

                _mockEventCategoryRepository.Verify(repo => repo.GetByIdAsync(updateModel.CategoryId), Times.Once);

            } else {

                _mockEventCategoryRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);

            }
            _mockMapper.Verify(mapper => mapper.Map(updateModel, existingEventEntity), Times.Once);

            _mockEventRepository.Verify(repo => repo.UpdateAsync(It.Is<EventEntity>(
                e => e.Id == updateModel.Id &&
                     e.Name == updateModel.Name &&
                     e.CategoryId == updateModel.CategoryId
            )), Times.Once);

            _mockMapper.Verify(mapper => mapper.Map<EventFullResponseModel>(updatedEntityFromRepoCall), Times.Once);
            mockDbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockDbContextTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        }

        [Theory]
        [AutoData]
        public async Task UpdateEventAsync_WhenEventToUpdateNotFound_ShouldThrowNotFoundExceptionAndRollback(
    UpdateEventRequestModel updateModel) {

            // Arrange
            var mockDbContextTransaction = _fixture.Create<Mock<IDbContextTransaction>>();

            _mockEventRepository.Setup(repo => repo.BeginTransactionAsync())
                .ReturnsAsync(mockDbContextTransaction.Object);
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(updateModel.Id))
                .ReturnsAsync((EventEntity?)null);
            mockDbContextTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _sut.UpdateEventAsync(updateModel);

            // Assert
            var exceptionAssertions = await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Resource 'Event' with identifier '{updateModel.Id}' not found.");
            exceptionAssertions.Which.ResourceName.Should().Be("Event");
            exceptionAssertions.Which.Identifier.ToString().Should().Be(updateModel.Id.ToString());

            mockDbContextTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockDbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockEventRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EventEntity>()), Times.Never);
        }

        [Theory]
        [AutoData]
        public async Task UpdateEventAsync_WhenNewCategoryNotFound_ShouldThrowNotFoundExceptionAndRollback(
    UpdateEventRequestModel updateModel) {

            // Arrange
            var existingEventEntity = _fixture.Build<EventEntity>()
                                             .With(e => e.Id, updateModel.Id)
                                             .Create();

            updateModel.CategoryId = _fixture.Create<Guid>();
            while (updateModel.CategoryId == Guid.Empty
                || updateModel.CategoryId == existingEventEntity.CategoryId) {
                updateModel.CategoryId = _fixture.Create<Guid>();
            }

            var mockDbContextTransaction = _fixture.Create<Mock<IDbContextTransaction>>();
            _mockEventRepository.Setup(repo => repo.BeginTransactionAsync())
                .ReturnsAsync(mockDbContextTransaction.Object);
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(updateModel.Id))
                .ReturnsAsync(existingEventEntity);
            _mockEventCategoryRepository.Setup(repo => repo.GetByIdAsync(updateModel.CategoryId))
                .ReturnsAsync((EventCategoryEntity?)null);
            mockDbContextTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _sut.UpdateEventAsync(updateModel);

            // Assert
            var exceptionAssertions = await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Resource 'Category' with identifier '{updateModel.CategoryId}' not found.");
            exceptionAssertions.Which.ResourceName
                .Should().Be("Category");
            exceptionAssertions.Which.Identifier.ToString()
                .Should().Be(updateModel.CategoryId.ToString());

            mockDbContextTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockDbContextTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _mockEventRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EventEntity>()), Times.Never);

        }

        [Theory]
        [AutoData]
        public async Task UpdateEventAsync_WhenRepositoryUpdateAsyncThrowsException_ShouldRollbackAndRethrow(
    UpdateEventRequestModel updateModel) {
            // Arrange
            var existingEventEntity = _fixture.Build<EventEntity>()
                                              .With(e => e.Id, updateModel.Id)
                                              .Create();

            if (updateModel.CategoryId != Guid.Empty && updateModel.CategoryId != existingEventEntity.CategoryId) {

                var categoryEntity = _fixture.Build<EventCategoryEntity>()
                    .With(c => c.Id, updateModel.CategoryId)
                    .Create();

                _mockEventCategoryRepository
                    .Setup(repo => repo.GetByIdAsync(updateModel.CategoryId))
                    .ReturnsAsync(categoryEntity);

            } else if (updateModel.CategoryId == Guid.Empty) {
                updateModel.CategoryId = existingEventEntity.CategoryId;
            }

            var repositoryException = new InvalidOperationException("Database update failed");
            var mockDbContextTransaction = _fixture.Create<Mock<IDbContextTransaction>>();

            _mockEventRepository.Setup(repo => repo.BeginTransactionAsync())
                .ReturnsAsync(mockDbContextTransaction.Object);

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(updateModel.Id))
                .ReturnsAsync(existingEventEntity);

            _mockMapper.Setup(mapper => mapper.Map(updateModel, existingEventEntity));

            _mockEventRepository
                .Setup(repo => repo.UpdateAsync(It.Is<EventEntity>(e => e.Id == updateModel.Id)))
                .ThrowsAsync(repositoryException);

            mockDbContextTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _sut.UpdateEventAsync(updateModel);

            // Assert
            var thrownException = ( await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database update failed") ).Which;
            thrownException.Should().BeSameAs(repositoryException);

            mockDbContextTransaction
                .Verify(
                    t => t.RollbackAsync(It.IsAny<CancellationToken>()),
                    Times.Once
                );

            mockDbContextTransaction
                .Verify(
                    t => t.CommitAsync(It.IsAny<CancellationToken>()),
                    Times.Never
                );

            _mockMapper
                .Verify(
                    mapper => mapper.Map<EventFullResponseModel>(It.IsAny<EventEntity>()),
                    Times.Never
                );

        }

        // =======================================================================
        // Тесты для DeleteEventByIdAsync
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task DeleteEventByIdAsync_WhenEventExists_ShouldCallRemoveRepository(Guid eventId) {

            // Arrange
            var eventEntity = _fixture.Build<EventEntity>().With(e => e.Id, eventId).Create();

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(eventEntity);
            _mockEventRepository.Setup(repo => repo.RemoveAsync(eventEntity))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteEventByIdAsync(eventId);

            // Assert
            _mockEventRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(repo => repo.RemoveAsync(eventEntity), Times.Once);

        }

        [Theory]
        [AutoData]
        public async Task DeleteEventByIdAsync_WhenEventNotFound_ShouldThrowNotFoundException(Guid eventId) {

            // Arrange
            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync((EventEntity?)null);

            // Act
            Func<Task> act = async () => await _sut.DeleteEventByIdAsync(eventId);

            // Assert
            var exceptionAssertions = await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Resource 'Event' with identifier '{eventId}' not found.");
            exceptionAssertions.Which.ResourceName.Should().Be("Event");
            exceptionAssertions.Which.Identifier.ToString().Should().Be(eventId.ToString());

            _mockEventRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(repo => repo.RemoveAsync(It.IsAny<EventEntity>()), Times.Never);
        }

        [Theory]
        [AutoData]
        public async Task DeleteEventByIdAsync_WhenRepositoryRemoveAsyncThrowsException_ShouldRethrow(Guid eventId) {

            // Arrange
            var eventEntity = _fixture.Build<EventEntity>().With(e => e.Id, eventId).Create();
            var repositoryException = new InvalidOperationException("Database delete failed");

            _mockEventRepository.Setup(repo => repo.GetByIdAsync(eventId))
                .ReturnsAsync(eventEntity);
            _mockEventRepository.Setup(repo => repo.RemoveAsync(eventEntity))
                .ThrowsAsync(repositoryException);

            // Act
            Func<Task> act = async () => await _sut.DeleteEventByIdAsync(eventId);

            // Assert
            var thrownException = ( await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database delete failed") ).Which;
            thrownException.Should().BeSameAs(repositoryException);

            _mockEventRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
            _mockEventRepository.Verify(repo => repo.RemoveAsync(eventEntity), Times.Once);

        }

        // =======================================================================
        // Тесты для DeleteEventByIdAsync
        // =======================================================================

    }

}
