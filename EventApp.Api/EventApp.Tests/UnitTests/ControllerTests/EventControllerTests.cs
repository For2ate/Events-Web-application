using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using EventApp.Api.Controllers;
using EventApp.Core.Exceptions;
using EventApp.Core.Interfaces;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;
using EventApp.Models.SharedDTO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        public async Task GetAllEvents_WithDefaultQueryParameters_ShouldReturnOkWithEventItemsAndPaginationHeaders_JsonNode() {
            // Arrange
            var queryParameters = _fixture.Create<EventQueryParameters>();
            var eventItems = _fixture.CreateMany<EventFullResponseModel>(queryParameters.PageSize).ToList();
            var totalCount = eventItems.Count + queryParameters.PageSize;
            var expectedPagedResponse = new PagedListResponse<EventFullResponseModel>(
                eventItems, queryParameters.PageNumber, queryParameters.PageSize, totalCount);

            _mockEventService.Setup(s => s.GetAllEventsAsync(queryParameters)).ReturnsAsync(expectedPagedResponse);

            // Act
            var result = await _sut.GetAllEvents(queryParameters);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedPagedResponse.Items);

            _sut.Response.Headers.Should().ContainKey("X-Pagination");
            var paginationHeaderJson = _sut.Response.Headers["X-Pagination"].FirstOrDefault();
            paginationHeaderJson.Should().NotBeNullOrEmpty();

            var expectedMetadata = new {
                expectedPagedResponse.TotalCount,
                expectedPagedResponse.PageSize,
                expectedPagedResponse.PageNumber,
                expectedPagedResponse.TotalPages,
                expectedPagedResponse.HasNextPage,
                expectedPagedResponse.HasPreviousPage
            };

            JsonNode? actualMetadataNode = JsonNode.Parse(paginationHeaderJson!);
            actualMetadataNode.Should().NotBeNull();
            JsonObject actualMetadataObject = actualMetadataNode!.AsObject();

            actualMetadataObject["TotalCount"]!.GetValue<int>().Should().Be(expectedMetadata.TotalCount);
            actualMetadataObject["PageSize"]!.GetValue<int>().Should().Be(expectedMetadata.PageSize);
            actualMetadataObject["PageNumber"]!.GetValue<int>().Should().Be(expectedMetadata.PageNumber);
            actualMetadataObject["TotalPages"]!.GetValue<int>().Should().Be(expectedMetadata.TotalPages);
            actualMetadataObject["HasNextPage"]!.GetValue<bool>().Should().Be(expectedMetadata.HasNextPage);
            actualMetadataObject["HasPreviousPage"]!.GetValue<bool>().Should().Be(expectedMetadata.HasPreviousPage);

            _mockEventService.Verify(s => s.GetAllEventsAsync(queryParameters), Times.Once);

        }

        [Fact]
        public async Task GetAllEvents_WhenServiceReturnsEmptyPagedList_ShouldReturnOkWithEmptyListAndPaginationHeaders() {
            // Arrange
            var queryParameters = new EventQueryParameters(); 
            var emptyEventItems = new List<EventFullResponseModel>();
            var totalCount = 0;

            var expectedPagedResponse = new PagedListResponse<EventFullResponseModel>(
                emptyEventItems,
                queryParameters.PageNumber,
                queryParameters.PageSize,
                totalCount
            );

            _mockEventService
                .Setup(s => s.GetAllEventsAsync(It.IsAny<EventQueryParameters>()))
                .ReturnsAsync(expectedPagedResponse);

            // Act
            var result = await _sut.GetAllEvents(queryParameters);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.As<IEnumerable<EventFullResponseModel>>().Should().BeEmpty();

            _mockEventService.Verify(s => s.GetAllEventsAsync(It.Is<EventQueryParameters>(qp =>
                qp.PageNumber == queryParameters.PageNumber && qp.PageSize == queryParameters.PageSize
            )), Times.Once);

            _sut.Response.Headers.Should().ContainKey("X-Pagination");
            var paginationHeaderJson = _sut.Response.Headers["X-Pagination"].ToString();
            JsonNode? actualMetadataNode = JsonNode.Parse(paginationHeaderJson!);
            actualMetadataNode.Should().NotBeNull();
            JsonObject actualMetadataObject = actualMetadataNode!.AsObject();

            actualMetadataObject["TotalCount"]!.GetValue<int>().Should().Be(0);
            actualMetadataObject["TotalPages"]!.GetValue<int>().Should().Be(0); 
            actualMetadataObject["HasNextPage"]!.GetValue<bool>().Should().BeFalse();
            actualMetadataObject["HasPreviousPage"]!.GetValue<bool>().Should().BeFalse();

        }

        // =======================================================================
        // Тесты для    GetEventById
        // =======================================================================

        [Fact]
        public async Task GetEventById_WhenEventExists_ShouldReturnOkWithEvent() {
            // Arrange
            var eventId = _fixture.Create<Guid>();
            var expectedEvent = _fixture.Create<EventFullResponseModel>();
            _mockEventService.Setup(s => s.GetEventByIdAsync(eventId)).ReturnsAsync(expectedEvent);

            // Act
            var result = await _sut.GetEventById(eventId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedEvent);
            _mockEventService.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetEventById_WhenServiceThrowsNotFoundException_ShouldRethrow() {
            // Arrange
            var eventId = _fixture.Create<Guid>();
            var serviceException = new NotFoundException("Event", eventId.ToString());
            _mockEventService.Setup(s => s.GetEventByIdAsync(eventId)).ThrowsAsync(serviceException);

            // Act
            Func<Task> act = async () => await _sut.GetEventById(eventId);

            // Assert
            await act.Should().ThrowExactlyAsync<NotFoundException>().WithMessage(serviceException.Message);
            _mockEventService.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
        }

        // =======================================================================
        // Тесты для    GetEventByName
        // =======================================================================

        [Fact]
        public async Task GetEventByName_WhenEventExists_ShouldReturnOkWithEvent() {
            // Arrange
            var eventName = _fixture.Create<string>();
            var expectedEvent = _fixture.Create<EventFullResponseModel>();
            _mockEventService.Setup(s => s.GetEventByNameAsync(eventName)).ReturnsAsync(expectedEvent);

            // Act
            var result = await _sut.GetEventByName(eventName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(expectedEvent);
            _mockEventService.Verify(s => s.GetEventByNameAsync(eventName), Times.Once);
        }

        [Fact]
        public async Task GetEventByName_WhenServiceThrowsNotFoundException_ShouldRethrow() {
            // Arrange
            var eventName = _fixture.Create<string>();
            var serviceException = new NotFoundException("Event", eventName);
            _mockEventService.Setup(s => s.GetEventByNameAsync(eventName)).ThrowsAsync(serviceException);

            // Act
            Func<Task> act = async () => await _sut.GetEventByName(eventName);

            // Assert
            await act.Should().ThrowExactlyAsync<NotFoundException>().WithMessage(serviceException.Message);
            _mockEventService.Verify(s => s.GetEventByNameAsync(eventName), Times.Once);
        }

        // =======================================================================
        // Тесты для    CreateEvent
        // =======================================================================

        [Fact]
        public async Task CreateEvent_ValidModel_ShouldReturnCreatedAtActionWithEvent() {
            // Arrange
            var requestModel = _fixture.Create<CreateEventRequestModel>();
            var createdEventDto = _fixture.Build<EventFullResponseModel>()
                                          .With(e => e.Id, Guid.NewGuid()) 
                                          .Create();
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

        [Fact]
        public async Task CreateEvent_WhenServiceThrowsBadRequestException_ShouldRethrow() {
            // Arrange
            var requestModel = _fixture.Create<CreateEventRequestModel>();
            var serviceException = new BadRequestException("Invalid event data.");
            _mockEventService.Setup(s => s.CreateEventAsync(requestModel)).ThrowsAsync(serviceException);

            // Act
            Func<Task> act = async () => await _sut.CreateEvent(requestModel);

            // Assert
            await act.Should().ThrowExactlyAsync<BadRequestException>().WithMessage(serviceException.Message);
            _mockEventService.Verify(s => s.CreateEventAsync(requestModel), Times.Once);
        }

        // =======================================================================
        // Тесты для    UpdateEvent
        // =======================================================================

        [Fact]
        public async Task UpdateEvent_ValidModel_ShouldReturnOkWithUpdatedEvent() {
            // Arrange
            var requestModel = _fixture.Create<UpdateEventRequestModel>();
            var updatedEventDto = _fixture.Build<EventFullResponseModel>()
                                          .With(e => e.Id, requestModel.Id) // Ensure ID matches
                                          .Create();
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ReturnsAsync(updatedEventDto);

            // Act
            var result = await _sut.UpdateEvent(requestModel);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            okResult.Value.Should().BeEquivalentTo(updatedEventDto);
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }

        [Fact]
        public async Task UpdateEvent_WhenServiceThrowsNotFoundException_ShouldRethrow() {
            // Arrange
            var requestModel = _fixture.Create<UpdateEventRequestModel>();
            var serviceException = new NotFoundException("Event", requestModel.Id.ToString());
            _mockEventService.Setup(s => s.UpdateEventAsync(requestModel)).ThrowsAsync(serviceException);

            // Act
            Func<Task> act = async () => await _sut.UpdateEvent(requestModel);

            // Assert
            await act.Should().ThrowExactlyAsync<NotFoundException>().WithMessage(serviceException.Message);
            _mockEventService.Verify(s => s.UpdateEventAsync(requestModel), Times.Once);
        }


        // =======================================================================
        // Тесты для    DeleteEvent
        // =======================================================================

        [Fact]
        public async Task DeleteEvent_WhenEventExists_ShouldReturnNoContent() {
            // Arrange
            var eventId = _fixture.Create<Guid>();
            _mockEventService.Setup(s => s.DeleteEventByIdAsync(eventId)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteEvent(eventId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockEventService.Verify(s => s.DeleteEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_WhenServiceThrowsNotFoundException_ShouldRethrow() {
            // Arrange
            var eventId = _fixture.Create<Guid>();
            var serviceException = new NotFoundException("Event", eventId.ToString());
            _mockEventService.Setup(s => s.DeleteEventByIdAsync(eventId)).ThrowsAsync(serviceException);

            // Act
            Func<Task> act = async () => await _sut.DeleteEvent(eventId);

            // Assert
            await act.Should().ThrowExactlyAsync<NotFoundException>().WithMessage(serviceException.Message);
            _mockEventService.Verify(s => s.DeleteEventByIdAsync(eventId), Times.Once);
        }

    }

}
