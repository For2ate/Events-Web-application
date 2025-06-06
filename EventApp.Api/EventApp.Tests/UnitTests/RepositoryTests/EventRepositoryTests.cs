﻿using AutoFixture;
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
                                   .Without(e => e.Category)
                                   .Without(e => e.Registrations).Create();

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
            
            var entities = _fixture.Build<EventEntity>()
                                   .Without(e => e.Category)       
                                   .Without(e => e.Registrations) 
                                   .CreateMany(3)
                                   .ToList();

            entities.ForEach(e => {
                if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>();
            });

            await _dbContext.Events.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllAsync(
                filter: null,
                orderBy: null,
                includeProperties: "",
                skip: null,
                take: null
            );

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(entities.Count);
            result.Should().BeEquivalentTo(entities, options => options
                .Excluding(e => e.Category)      
                .Excluding(e => e.Registrations)
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenNoEntitiesExist_ShouldReturnEmptyCollection() {
            
            // Arrange

            //Act
            var result = await _sut.GetAllAsync(
                filter: null,
                orderBy: null,
                includeProperties: "",
                skip: null,
                take: null
            );


            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithFilter_ShouldReturnFilteredEntities() {

            // Arrange

            var specificName = "Test Event For Filter";
            var entitiesToKeep = _fixture.Build<EventEntity>()
                .Without(e => e.Category)
                .Without(e => e.Registrations)
                .With(e => e.Name, specificName)
                .CreateMany(2).ToList();

            var entitiesToDiscard = _fixture.Build<EventEntity>()
                .Without(e => e.Category)
                .Without(e => e.Registrations)
                .With(e => e.Name, "Other Event")
                .CreateMany(3).ToList();

            entitiesToKeep.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });
            entitiesToDiscard.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });

            await _dbContext.Events.AddRangeAsync(entitiesToKeep);
            await _dbContext.Events.AddRangeAsync(entitiesToDiscard);
            await _dbContext.SaveChangesAsync();
            
            //Act
            var result = await _sut.GetAllAsync(filter: e => e.Name == specificName);

            //Assert
            result.Should().HaveCount(entitiesToKeep.Count);
            result.Should().OnlyContain(e => e.Name == specificName);
            result.Should().BeEquivalentTo(entitiesToKeep, options => options
                .Excluding(e => e.Category)
                .Excluding(e => e.Registrations)
            );
        }

        [Fact]
        public async Task GetAllAsync_WithPagination_ShouldReturnPagedEntities() {
            
            //Arrange
            var entities = _fixture.Build<EventEntity>()
                .Without(e => e.Category)
                .Without(e => e.Registrations)
                .CreateMany(5).ToList(); 
            entities.ForEach(e => { if (e.CategoryId == Guid.Empty) e.CategoryId = _fixture.Create<Guid>(); });

            await _dbContext.Events.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();

            int pageNumber = 2;
            int pageSize = 2;
            
            //Act
            var result = await _sut.GetAllAsync(
                orderBy: q => q.OrderBy(e => e.Name), 
                skip: ( pageNumber - 1 ) * pageSize,
                take: pageSize
            );

            result.Should().HaveCount(pageSize);
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

            var eventName = _fixture.Create<string>();
            var categoryId = _fixture.Create<Guid>(); 

            var category = _fixture.Build<EventCategoryEntity>()
                                   .With(c => c.Id, categoryId)
                                   .Without(c => c.Events)
                                   .Create();

            var expectedEntity = _fixture.Build<EventEntity>()
                                         .With(e => e.Name, eventName)
                                         .With(e => e.CategoryId, categoryId) 
                                         .Without(e => e.Category)    
                                         .Without(e => e.Registrations)
                                         .Create();

            _dbContext.Events.Add(expectedEntity);
            await _dbContext.SaveChangesAsync();

            _dbContext.ChangeTracker.Clear();

            // Act
            var eventRepository = new EventRepository(_dbContext);
            var result = await eventRepository.GetEventByNameAsync(eventName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedEntity, options => options
                .Excluding(e => e.Category)       
                .Excluding(e => e.Registrations)  
            );

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

    }

}
