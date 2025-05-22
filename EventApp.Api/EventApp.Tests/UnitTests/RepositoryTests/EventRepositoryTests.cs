using AutoFixture;
using AutoFixture.AutoMoq;
using EventApp.Data.DbContexts;
using EventApp.Data.Entities;
using EventApp.Data.Repositories;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.SharedDTO;
using EventApp.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Tests.UnitTests.RepositoryTests {

    public class EventRepositoryTests {

        private readonly ApplicationContext _dbContext;
        private readonly EventRepository _sut;
        private readonly IFixture _fixture;

        public EventRepositoryTests() {
            
            _dbContext = DbContextHelper.CreateInMemoryDbContext();       

            _sut = new EventRepository(_dbContext);

            _fixture = new Fixture();

            _fixture = new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new OmitOnRecursionBehaviorCustomization());

        }

        private class OmitOnRecursionBehaviorCustomization : ICustomization {
            public void Customize(IFixture fixture) {
                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }
        }

        public void Dispose() {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this); 
        }

        // =======================================================================
        // Тесты для   GetByIdAsync
        // =======================================================================

        [Fact]
        public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity() {

            //Arrange
            var expectedEntity = _fixture.Build<EventEntity>()
                                         .Create();

            if (expectedEntity.CategoryId == Guid.Empty) {
                expectedEntity.CategoryId = _fixture.Create<Guid>(); 
            }


            _dbContext.Events.Add(expectedEntity);
            await _dbContext.SaveChangesAsync(); 

            // Act
            var result = await _sut.GetByIdAsync(expectedEntity.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedEntity);

        }

        [Fact]
        public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull() {
            
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _sut.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();

        }


        // =======================================================================
        // Тесты для    GetAllAsync
        // =======================================================================

        [Fact]
        public async Task GetAllAsync_WhenEntitiesExist_ShouldReturnAllEntities() {
            
            // Arrange          
            var entities = _fixture.CreateMany<EventEntity>(3).ToList();
            entities.ForEach(e => {
                if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>();
            });

            _dbContext.Events.AddRange(entities);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(entities);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoEntitiesExist_ShouldReturnEmptyCollection() {
            // Arrange

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // =======================================================================
        // Тесты для   AddAsync
        // =======================================================================

        [Fact]
        public async Task AddAsync_ShouldAddEntityToDatabase() {
            
            // Arrange
            var newEntity = _fixture.Build<EventEntity>()
                                    .Create();
            if (newEntity.CategoryId == Guid.Empty) newEntity.CategoryId = _fixture.Create<Guid>();


            // Act
            await _sut.AddAsync(newEntity);

            // Assert
            var entityInDb = await _dbContext.Events.FindAsync(newEntity.Id);
            entityInDb.Should().NotBeNull();
            entityInDb.Should().BeEquivalentTo(newEntity);

        }

        // =======================================================================
        // Тесты для   UpdateAsync
        // =======================================================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntityInDatabase() {
            
            // Arrange
            var originalEntity = _fixture.Build<EventEntity>().Create();
            if (originalEntity.CategoryId == Guid.Empty) originalEntity.CategoryId = _fixture.Create<Guid>();

            _dbContext.Events.Add(originalEntity);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(originalEntity).State = EntityState.Detached;

            var updatedName = _fixture.Create<string>(); 
            var updatedDescription = _fixture.Create<string>(); 

            var entityToUpdate = _fixture.Build<EventEntity>()
                .With(e => e.Id, originalEntity.Id) 
                .With(e => e.Name, updatedName)
                .With(e => e.Description, updatedDescription)
                .With(e => e.DateOfEvent, originalEntity.DateOfEvent)
                .With(e => e.MaxNumberOfParticipants, originalEntity.MaxNumberOfParticipants)
                .With(e => e.ImageUrl, originalEntity.ImageUrl) 
                .With(e => e.Place, originalEntity.Place) 
                .With(e => e.CategoryId, originalEntity.CategoryId) 
                .Create();


            // Act
            await _sut.UpdateAsync(entityToUpdate);

            // Assert
            var entityInDb = await _dbContext.Events.FindAsync(originalEntity.Id);
            entityInDb.Should().NotBeNull();
            entityInDb.Name.Should().Be(updatedName);
            entityInDb.Description.Should().Be(updatedDescription);

        }

        // =======================================================================
        // Тесты для    RemoveAsync
        // =======================================================================

        [Fact]
        public async Task RemoveAsync_ShouldRemoveEntityFromDatabase() {
            
            // Arrange
            var entityToRemove = _fixture.Build<EventEntity>().Create();
            if (entityToRemove.CategoryId == Guid.Empty) entityToRemove.CategoryId = _fixture.Create<Guid>();

            _dbContext.Events.Add(entityToRemove);
            await _dbContext.SaveChangesAsync();

            // Act
            await _sut.RemoveAsync(entityToRemove);

            // Assert
            var entityInDb = await _dbContext.Events.FindAsync(entityToRemove.Id);
            entityInDb.Should().BeNull();

        }

        // =======================================================================
        // Тесты для    GetEventByNameAsync
        // =======================================================================

        [Fact]
        public async Task GetEventByNameAsync_WhenEventWithGivenNameExists_ShouldReturnEntity() {
            
            // Arrange
            var eventName = _fixture.Create<string>(); 
            var expectedEntity = _fixture.Build<EventEntity>()
                                         .With(e => e.Name, eventName)
                                         .Create();
            if (expectedEntity.CategoryId == Guid.Empty) expectedEntity.CategoryId = _fixture.Create<Guid>();

            var otherEntities = _fixture.Build<EventEntity>()
                                        .Without(e => e.Name) 
                                        .CreateMany(2).ToList();
            otherEntities.ForEach(e => {
                e.Name = _fixture.Create<string>(); 
                if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>();
            });

            _dbContext.Events.Add(expectedEntity);
            _dbContext.Events.AddRange(otherEntities);
            await _dbContext.SaveChangesAsync();

            // Act
           
            var eventRepository = new EventRepository(_dbContext); 
            var result = await eventRepository.GetEventByNameAsync(eventName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedEntity);

        }

        [Fact]
        public async Task GetEventByNameAsync_WhenEventWithGivenNameDoesNotExist_ShouldReturnNull() {
            
            // Arrange
            var searchName = _fixture.Create<string>();
            var otherEntities = _fixture.Build<EventEntity>()
                                        .CreateMany(3).ToList();

            otherEntities.ForEach(e => { 
                while (e.Name == searchName) e.Name = _fixture.Create<string>();
                if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>();
            });

            _dbContext.Events.AddRange(otherEntities);
            await _dbContext.SaveChangesAsync();

            // Act
            var eventRepository = new EventRepository(_dbContext);
            var result = await eventRepository.GetEventByNameAsync(searchName);

            // Assert
            result.Should().BeNull();

        }

        [Fact]
        public async Task GetEventByNameAsync_WhenNameIsCaseSensitiveAndMatchExists_ShouldReturnEntity() {
            
            // Arrange

            var eventName = "SpecificEventName";
            var expectedEntity = _fixture.Build<EventEntity>()
                                         .With(e => e.Name, eventName)
                                         .Create();
            if (expectedEntity.CategoryId == Guid.Empty) expectedEntity.CategoryId = _fixture.Create<Guid>();

            _dbContext.Events.Add(expectedEntity);
            await _dbContext.SaveChangesAsync();

            // Act
            var eventRepository = new EventRepository(_dbContext);
            var result = await eventRepository.GetEventByNameAsync(eventName);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(eventName);
        }

        [Fact]
        public async Task GetEventByNameAsync_WhenNameIsCaseSensitiveAndNoExactMatch_ShouldReturnNull() {
           
            // Arrange
            var eventNameInDb = "SpecificEventName";
            var searchName = "specificeventname"; 

            var entityInDb = _fixture.Build<EventEntity>()
                                     .With(e => e.Name, eventNameInDb)
                                     .Create();

            if (entityInDb.CategoryId == Guid.Empty) entityInDb.CategoryId = _fixture.Create<Guid>();

            _dbContext.Events.Add(entityInDb);
            await _dbContext.SaveChangesAsync();

            // Act
            var eventRepository = new EventRepository(_dbContext);
            var result = await eventRepository.GetEventByNameAsync(searchName);

            // Assert
            result.Should().BeNull(); 
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetEventByNameAsync_WhenNameIsNullOrEmpty_ShouldReturnNull(string? searchName) {
            
            // Arrange
           
            var entities = _fixture.Build<EventEntity>().CreateMany(2).ToList();
            entities.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });
            _dbContext.Events.AddRange(entities);
            await _dbContext.SaveChangesAsync();

            // Act
            var eventRepository = new EventRepository(_dbContext);
            var result = await eventRepository.GetEventByNameAsync(searchName!);

            // Assert
            result.Should().BeNull();

        }

        // =======================================================================
        // Тесты для    GetFilteredEventsAsync
        // =======================================================================

        private async Task SeedData(IEnumerable<EventEntity> events) {
            _dbContext.Events.AddRange(events);
            await _dbContext.SaveChangesAsync();
        }
        private async Task SeedCategories(IEnumerable<EventCategoryEntity> categories) {
            _dbContext.EventsCategories.AddRange(categories);
            await _dbContext.SaveChangesAsync();
        }


        [Fact]
        public async Task GetFilteredEventsAsync_WithPagination_ShouldReturnCorrectPage() {
            // Arrange
            var allEvents = _fixture.Build<EventEntity>()
                                    .Do(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); }) // Убедимся в валидности CategoryId
                                    .CreateMany(25).ToList(); 

            await SeedData(allEvents);

            var queryParameters = new EventQueryParameters {
                PageNumber = 2,
                PageSize = 10
            };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(25);
            events.Should().HaveCount(10);
            var expectedPageContent = allEvents.OrderBy(e => e.DateOfEvent).Skip(10).Take(10).ToList();
            events.Should().BeEquivalentTo(expectedPageContent, options => options.Excluding(e => e.Category).Excluding(e => e.Registrations));
        
        }

        [Fact]
        public async Task GetFilteredEventsAsync_WithCategoryFilter_ShouldReturnMatchingEvents() {
            
            // Arrange
            var targetCategory = _fixture.Build<EventCategoryEntity>()
                                            .CreateMany(2)
                                            .ToList();

            await SeedCategories(targetCategory);

            var matchingEvents = _fixture.Build<EventEntity>()
                                         .With(e => e.CategoryId, targetCategory[0].Id)
                                         .CreateMany(3)
                                         .ToList();

            var nonMatchingEvents = _fixture.Build<EventEntity>()
                                            .With(e => e.CategoryId, targetCategory[1].Id)
                                            .CreateMany(2)
                                            .ToList();

            await SeedData(matchingEvents.Concat(nonMatchingEvents));

            var queryParameters = new EventQueryParameters { CategoryId = targetCategory[0].Id, PageSize = 5 };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(3);
            events.Should().HaveCount(3);
            events.Should().OnlyContain(e => e.CategoryId == targetCategory[0].Id);
        }

        [Fact]
        public async Task GetFilteredEventsAsync_WithDateFilter_ShouldReturnEventsInDateRange() {
            
            // Arrange
            var dateFrom = new DateTime(2023, 10, 15);
            var dateTo = new DateTime(2023, 10, 20);

            var eventsInRange = new List<EventEntity> {
                _fixture.Build<EventEntity>().With(e => e.DateOfEvent, new DateTime(2023, 10, 15, 10, 0, 0)).Create(),
                _fixture.Build<EventEntity>().With(e => e.DateOfEvent, new DateTime(2023, 10, 20, 18, 0, 0)).Create()
            };

            var eventsOutOfRange = new List<EventEntity> {
                _fixture.Build<EventEntity>().With(e => e.DateOfEvent, new DateTime(2023, 10, 14)).Create(),
                _fixture.Build<EventEntity>().With(e => e.DateOfEvent, new DateTime(2023, 10, 21)).Create()
            };

            eventsInRange.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });
            eventsOutOfRange.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });

            await SeedData(eventsInRange.Concat(eventsOutOfRange));

            var queryParameters = new EventQueryParameters { DateFrom = dateFrom, DateTo = dateTo, PageSize = 5 };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(2);
            events.Should().HaveCount(2);
            events.Should().OnlyContain(e => e.DateOfEvent >= dateFrom && e.DateOfEvent <= dateTo.AddDays(1).AddTicks(-1));
        
        }

        [Fact]
        public async Task GetFilteredEventsAsync_WithPlaceFilter_ShouldReturnMatchingEvents_InMemoryLimitations() {
            // Arrange
            var targetPlaceSubstring = "Hall";
            var matchingEvents = new List<EventEntity> {
                _fixture.Build<EventEntity>().With(e => e.Place, "Main Hall A").Create(),
                _fixture.Build<EventEntity>().With(e => e.Place, "Convention Hall B").Create()
            };

            var nonMatchingEvents = new List<EventEntity> {
                _fixture.Build<EventEntity>().With(e => e.Place, "Room 101").Create()
            };
            matchingEvents.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });
            nonMatchingEvents.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });
            await SeedData(matchingEvents.Concat(nonMatchingEvents));

            var queryParameters = new EventQueryParameters { Place = targetPlaceSubstring, PageSize = 5 };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(2);
            events.Should().HaveCount(2);
            events.Should().OnlyContain(e => e.Place.Contains(targetPlaceSubstring, StringComparison.OrdinalIgnoreCase));

        }

        [Fact]
        public async Task GetFilteredEventsAsync_WithSortByNameAsc_ShouldReturnSortedEvents() {
            // Arrange
            var eventsToSort = new List<EventEntity> {
                _fixture.Build<EventEntity>().With(e => e.Name, "Charlie Event").Do(e => e.CategoryId = _fixture.Create<Guid>()).Create(),
                _fixture.Build<EventEntity>().With(e => e.Name, "Alpha Event").Do(e => e.CategoryId = _fixture.Create<Guid>()).Create(),
                _fixture.Build<EventEntity>().With(e => e.Name, "Bravo Event").Do(e => e.CategoryId = _fixture.Create<Guid>()).Create()
            };
            await SeedData(eventsToSort);

            var queryParameters = new EventQueryParameters {
                SortBy = SortByEnums.name, // Используем enum
                SortOrder = SortOrderEnum.ask,
                PageSize = 3
            };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(3);
            events.Should().HaveCount(3);
            events.Should().BeInAscendingOrder(e => e.Name);
        }

        [Fact]
        public async Task GetFilteredEventsAsync_WithSortByCategoryNameDesc_ShouldReturnSortedEvents() {

            // Arrange
            var category1 = _fixture.Build<EventCategoryEntity>()
                                    .With(c => c.Name, "Xy")
                                    .Without(e => e.Events)
                                    .Create();
            var category2 = _fixture.Build<EventCategoryEntity>()
                                    .With(c => c.Name, "A")
                                    .Without(e => e.Events)
                                    .Create();
            var category3 = _fixture.Build<EventCategoryEntity>()
                                    .Without(e => e.Events)
                                    .With(c => c.Name, "M")
                                    .Create();

            await SeedCategories(new List<EventCategoryEntity> { category1, category2, category3 });


            var eventsToSort = new List<EventEntity>{
                _fixture.Build<EventEntity>()
                        .With(e => e.CategoryId, category1.Id)
                        .With(e => e.Name, "Event C")
                        .Create(),

                _fixture.Build<EventEntity>()
                        .With(e => e.CategoryId, category2.Id)
                        .With(e => e.Name, "Event A")
                        .Create(),

                _fixture.Build<EventEntity>()
                        .With(e => e.CategoryId, category3.Id)
                        .With(e => e.Name, "Event B")
                        .Create()
            };

            await SeedData(eventsToSort);

            var queryParameters = new EventQueryParameters {
                SortBy = SortByEnums.category,
                SortOrder = SortOrderEnum.desc,
                PageSize = 6
            };

            // Act
            var (events, totalCount) = await _sut.GetFilteredEventsAsync(queryParameters);

            // Assert
            totalCount.Should().Be(3);
            events.Should().HaveCount(3);
            events.Select(e => e.Category?.Name).Should().NotContainNulls();
            events.Should().BeInDescendingOrder(e => e.Category.Name);

        }

    }

}
