using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using EventApp.Api.Controllers;
using EventApp.Api.Core.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using EventApp.Models.SharedDTO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventApp.Tests.UnitTests.ControllerTests {

    public class EventControllerTests {

        private readonly IFixture _fixture;
        private readonly Mock<IEventService> _mockEventService;
        private readonly EventController _sut;

        public EventControllerTests() {

            _fixture = new Fixture();

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });


            _mockEventService = _fixture.Freeze<Mock<IEventService>>();
            _sut = new EventController(_mockEventService.Object);

            SetupControllerContext();
        }

        private void SetupControllerContext() {
            var httpContext = new DefaultHttpContext();
            _sut.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext()
            };
        }

        // =======================================================================
        // Тесты для    GetAllEvents
        // =======================================================================

        [Fact]
        public async Task GetAllEvents_ShouldReturnOkWithEvents_WhenServiceReturnsEvents() {
            // Arrange
            var expectedEvents = _fixture.CreateMany<EventFullResponseModel>().ToList();
            _mockEventService.Setup(s => s.GetAllEventsAsync()).ReturnsAsync(expectedEvents);

            // Act
            var result = await _sut.GetAllEvents();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedEvents);
            _mockEventService.Verify(s => s.GetAllEventsAsync(), Times.Once);
        }

        // =======================================================================
        // Тесты для    GetFilteredEvents
        // =======================================================================


        [Theory]
        [AutoData]
        public async Task GetFilteredEvents_ShouldReturnOkWithPaginatedItemsAndHeaders_WhenServiceReturnsPagedResult(
    EventQueryParameters queryParameters) {
            
            // Arrange
            var pagedResultFromService = _fixture.Create<PagedListResponse<EventFullResponseModel>>();
            _mockEventService.Setup(s => s.GetFilteredEventsAsync(queryParameters))
                             .ReturnsAsync(pagedResultFromService);

            // Act
            var result = await _sut.GetFilteredEvents(queryParameters);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(pagedResultFromService.Items);

            _mockEventService.Verify(s => s.GetFilteredEventsAsync(queryParameters), Times.Once);

            _sut.Response.Headers.Should().ContainKey("X-Pagination");
            var paginationHeader = _sut.Response.Headers["X-Pagination"].ToString();
            paginationHeader.Should().Contain($"\"TotalCount\":{pagedResultFromService.TotalCount}");
        
        }

        // =======================================================================
        // Тесты для    GetEventById
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task GetEventById_ShouldReturnOkWithEvent_WhenEventExists(Guid id) {
            
            // Arrange
            var expectedEvent = _fixture.Create<EventFullResponseModel>();
            _mockEventService.Setup(s => s.GetEventByIdAsync(id)).ReturnsAsync(expectedEvent);

            // Act
            var result = await _sut.GetEventById(id);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedEvent);
            _mockEventService.Verify(s => s.GetEventByIdAsync(id), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetEventById_ShouldReturnNotFound_WhenEventDoesNotExist(Guid id) {
            // Arrange
            _mockEventService.Setup(s => s.GetEventByIdAsync(id)).ReturnsAsync((EventFullResponseModel?)null);

            // Act
            var result = await _sut.GetEventById(id);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Event with ID {id} not found.");
            _mockEventService.Verify(s => s.GetEventByIdAsync(id), Times.Once);
        }

        // =======================================================================
        // Тесты для    GetEventByName
        // =======================================================================

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetEventByName_ShouldReturnBadRequest_WhenNameIsNullOrWhitespace(string? name) {
            // Arrange
            // No service call expected

            // Act
            var result = await _sut.GetEventByName(name!);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be("Event name cannot be empty.");
            _mockEventService.Verify(s => s.GetEventByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [AutoData]
        public async Task GetEventByName_ShouldReturnOkWithEvent_WhenEventExists(string name) {
            // Arrange
            var expectedEvent = _fixture.Create<EventFullResponseModel>();
            _mockEventService.Setup(s => s.GetEventByNameAsync(name)).ReturnsAsync(expectedEvent);

            // Act
            var result = await _sut.GetEventByName(name);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedEvent);
            _mockEventService.Verify(s => s.GetEventByNameAsync(name), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetEventByName_ShouldReturnNotFound_WhenServiceReturnsNull(string name) {
            // Arrange
            _mockEventService.Setup(s => s.GetEventByNameAsync(name)).ReturnsAsync((EventFullResponseModel?)null);

            // Act
            var result = await _sut.GetEventByName(name);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Event with name '{name}' not found.");
            _mockEventService.Verify(s => s.GetEventByNameAsync(name), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task GetEventByName_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException(string name) {
            // Arrange
            var exceptionMessage = "Specific key not found message";
            _mockEventService.Setup(s => s.GetEventByNameAsync(name)).ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _sut.GetEventByName(name);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be(exceptionMessage);
            _mockEventService.Verify(s => s.GetEventByNameAsync(name), Times.Once);
        }

        // =======================================================================
        // Тесты для    CreateEvent
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task CreateEvent_ShouldReturnCreatedAtAction_WhenCreationIsSuccessful(CreateEventRequestModel requestModel) {
            // Arrange
            var createdEventDto = _fixture.Create<EventFullResponseModel>();
            // Гарантируем, что у созданного DTO есть Id для проверки CreatedAtAction
            createdEventDto.Id = _fixture.Create<Guid>();
            _mockEventService.Setup(s => s.CreateEventAsync(requestModel)).ReturnsAsync(createdEventDto);

            // Act
            var result = await _sut.CreateEvent(requestModel);

            // Assert
            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdAtActionResult.ActionName.Should().Be(nameof(EventController.GetEventById));
            createdAtActionResult.RouteValues?["id"].Should().Be(createdEventDto.Id);
            createdAtActionResult.Value.Should().BeEquivalentTo(createdEventDto);
            _mockEventService.Verify(s => s.CreateEventAsync(requestModel), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task CreateEvent_ShouldReturnBadRequest_WhenServiceThrowsArgumentException(CreateEventRequestModel requestModel) {
            // Arrange
            var exceptionMessage = "Invalid category ID";
            _mockEventService.Setup(s => s.CreateEventAsync(requestModel)).ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _sut.CreateEvent(requestModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be(exceptionMessage);
            _mockEventService.Verify(s => s.CreateEventAsync(requestModel), Times.Once);
        }

        // =======================================================================
        // Тесты для    UpdateEvent
        // =======================================================================

        [Theory]
        [AutoData]
        public async Task UpdateEvent_ShouldReturnOkWithUpdatedEvent_WhenUpdateIsSuccessful(UpdateEventRequestModel requestModel) {
            // Arrange
            var updatedEventDto = _fixture.Create<EventFullResponseModel>();
            updatedEventDto.Id = requestModel.Id;
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ReturnsAsync(updatedEventDto);

            // Act
            var result = await _sut.UpdateEvent(requestModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(updatedEventDto);
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task UpdateEvent_ShouldReturnNotFound_WhenServiceReturnsNull(UpdateEventRequestModel requestModel) {
            // Arrange
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ReturnsAsync((EventFullResponseModel?)null);

            // Act
            var result = await _sut.UpdateEvent(requestModel);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be($"Event with ID {requestModel.Id} not found for update.");
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task UpdateEvent_ShouldReturnBadRequest_WhenServiceThrowsArgumentException(UpdateEventRequestModel requestModel) {
            // Arrange
            var exceptionMessage = "Invalid data for update";
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ThrowsAsync(new ArgumentException(exceptionMessage));

            // Act
            var result = await _sut.UpdateEvent(requestModel);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            badRequestResult.Value.Should().Be(exceptionMessage);
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task UpdateEvent_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException(UpdateEventRequestModel requestModel) {
            // Arrange
            var exceptionMessage = "Underlying resource not found";
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _sut.UpdateEvent(requestModel);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be(exceptionMessage);
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }

        // =======================================================================
        // Тесты для    DeleteEvent
        // =======================================================================

        // Внутри EventControllerTests

        [Theory]
        [AutoData]
        public async Task DeleteEvent_ShouldReturnNoContent_WhenDeletionIsSuccessful(Guid id) {
            
            // Arrange
            _mockEventService.Setup(s => s.DeleteEventByIdAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteEvent(id);

            // Assert
            var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
            noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _mockEventService.Verify(s => s.DeleteEventByIdAsync(id), Times.Once);
        }

        [Theory]
        [AutoData]
        public async Task DeleteEvent_ShouldReturnNotFound_WhenServiceThrowsKeyNotFoundException(Guid id) {
           
            // Arrange
            var exceptionMessage = $"Event with ID {id} not found to delete";
            _mockEventService.Setup(s => s.DeleteEventByIdAsync(id)).ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _sut.DeleteEvent(id);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            notFoundResult.Value.Should().Be(exceptionMessage);
            _mockEventService.Verify(s => s.DeleteEventByIdAsync(id), Times.Once);
        
        }

    }

}
