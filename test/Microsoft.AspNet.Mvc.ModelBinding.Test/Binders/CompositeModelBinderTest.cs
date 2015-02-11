// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class CompositeModelBinderTest
    {
        [Fact]
        public async Task BindModel_SuccessfulBind_ReturnsModel()
        {
            // Arrange
            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someName", "dummyValue" }
                },
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = GetValidatorProvider()
                }
            };

            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate (ModelBindingContext context)
                    {
                        Assert.Same(bindingContext.ModelMetadata, context.ModelMetadata);
                        Assert.Equal("someName", context.ModelName);
                        Assert.Same(bindingContext.ValueProvider, context.ValueProvider);

                        context.Model = 42;
                        return Task.FromResult(true);
                    });
            var shimBinder = CreateCompositeBinder(mockIntBinder.Object);

            // Act
            var isBound = await shimBinder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(42, bindingContext.Model);
        }

        [Fact]
        public async Task BindModel_SuccessfulBind_ComplexTypeFallback_ReturnsModel()
        {
            // Arrange
            var expectedModel = new List<int> { 1, 2, 3, 4, 5 };

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someOtherName", "dummyValue" }
                },
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = GetValidatorProvider()
                }
            };

            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(
                    delegate (ModelBindingContext mbc)
                    {
                        if (!string.IsNullOrEmpty(mbc.ModelName))
                        {
                            return Task.FromResult(false);
                        }

                        Assert.Same(bindingContext.ModelMetadata, mbc.ModelMetadata);
                        Assert.Equal("", mbc.ModelName);
                        Assert.Same(bindingContext.ValueProvider, mbc.ValueProvider);

                        mbc.Model = expectedModel;
                        //mbc.ValidationNode.Validating += delegate { validationCalled = true; };
                        return Task.FromResult(true);
                    });

            var shimBinder = CreateCompositeBinder(mockIntBinder.Object);

            // Act
            var isBound = await shimBinder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);
            Assert.Equal(expectedModel, bindingContext.Model);
        }

        [Fact]
        public async Task ModelBinder_ReturnsTrue_WithoutSettingValue_DoesNotSetTheModelStateKey()
        {
            // Arrange

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someOtherName", "dummyValue" }
                },
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = GetValidatorProvider()
                }
            };

            var modelBinder = new Mock<IModelBinder>();
            modelBinder
                .Setup(mb => mb.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(Task.FromResult(true));

            var composite = CreateCompositeBinder(modelBinder.Object);

            // Act
            var isBound = await composite.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);

            Assert.Null(bindingContext.Model);
            Assert.False(bindingContext.IsModelSet);
            Assert.Null(bindingContext.ModelStateKey);
        }

        [Fact]
        public async Task ModelBinder_ReturnsTrue_SetsNullValue_SetsModelStateKey()
        {
            // Arrange

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new SimpleHttpValueProvider
                {
                    { "someOtherName", "dummyValue" }
                },
                OperationBindingContext = new OperationBindingContext
                {
                    ValidatorProvider = GetValidatorProvider()
                }
            };

            var modelBinder = new Mock<IModelBinder>();
            modelBinder
                .Setup(mb => mb.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Callback<ModelBindingContext>(context =>
                {
                    context.Model = null;
                })
                .Returns(Task.FromResult(true));

            var composite = CreateCompositeBinder(modelBinder.Object);

            // Act
            var isBound = await composite.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);

            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.IsModelSet);
            Assert.Equal("someName", bindingContext.ModelStateKey);
        }

        [Fact]
        public async Task BindModel_UnsuccessfulBind_BinderFails_ReturnsNull()
        {
            // Arrange
            var mockListBinder = new Mock<IModelBinder>();
            mockListBinder.Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                          .Returns(Task.FromResult(false))
                          .Verifiable();

            var shimBinder = mockListBinder.Object;

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = false,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(List<int>)),
            };

            // Act
            var isBound = await shimBinder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
            Assert.True(bindingContext.ModelState.IsValid);
            mockListBinder.Verify();
        }

        [Fact]
        public async Task BindModel_UnsuccessfulBind_SimpleTypeNoFallback_ReturnsNull()
        {
            // Arrange
            var innerBinder = Mock.Of<IModelBinder>();
            var shimBinder = CreateCompositeBinder(innerBinder);

            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = Mock.Of<OperationBindingContext>(),
            };

            // Act
            var isBound = await shimBinder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(isBound);
            Assert.Null(bindingContext.Model);
        }

        [Fact]
        public async Task BindModel_WithDefaultBinders_BindsSimpleType()
        {
            // Arrange
            var binder = CreateBinderWithDefaults();

            var valueProvider = new SimpleHttpValueProvider
            {
                { "firstName", "firstName-value"},
                { "lastName", "lastName-value"}
            };
            var bindingContext = CreateBindingContext(binder, valueProvider, typeof(SimplePropertiesModel));

            // Act
            var isBound = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);
            var model = Assert.IsType<SimplePropertiesModel>(bindingContext.Model);
            Assert.Equal("firstName-value", model.FirstName);
            Assert.Equal("lastName-value", model.LastName);
        }

        [Fact]
        public async Task BindModel_WithDefaultBinders_BindsComplexType()
        {
            // Arrange
            var binder = CreateBinderWithDefaults();

            var valueProvider = new SimpleHttpValueProvider
            {
                { "firstName", "firstName-value"},
                { "lastName", "lastName-value"},
                { "friends[0].firstName", "first-friend"},
                { "friends[0].age", "40"},
                { "friends[0].friends[0].firstname", "nested friend"},
                { "friends[1].firstName", "some other"},
                { "friends[1].lastName", "name"},
                { "resume", "4+mFeTp3tPF=" }
            };
            var bindingContext = CreateBindingContext(binder, valueProvider, typeof(Person));

            // Act
            var isBound = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(isBound);
            var model = Assert.IsType<Person>(bindingContext.Model);
            Assert.Equal("firstName-value", model.FirstName);
            Assert.Equal("lastName-value", model.LastName);
            Assert.Equal(2, model.Friends.Count);
            Assert.Equal("first-friend", model.Friends[0].FirstName);
            Assert.Equal(40, model.Friends[0].Age);
            var nestedFriend = Assert.Single(model.Friends[0].Friends);
            Assert.Equal("nested friend", nestedFriend.FirstName);
            Assert.Equal("some other", model.Friends[1].FirstName);
            Assert.Equal("name", model.Friends[1].LastName);
            Assert.Equal(new byte[] { 227, 233, 133, 121, 58, 119, 180, 241 }, model.Resume);
        }

        private static ModelBindingContext CreateBindingContext(IModelBinder binder,
                                                                IValueProvider valueProvider,
                                                                Type type,
                                                                IModelValidatorProvider validatorProvider = null)
        {
            validatorProvider = validatorProvider ?? GetValidatorProvider();
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var bindingContext = new ModelBindingContext
            {
                FallbackToEmptyPrefix = true,
                ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
                OperationBindingContext = new OperationBindingContext
                {
                    MetadataProvider = metadataProvider,
                    ModelBinder = binder,
                    ValidatorProvider = validatorProvider
                }
            };
            return bindingContext;
        }

        private static CompositeModelBinder CreateBinderWithDefaults()
        {
            var serviceProvider = Mock.Of<IServiceProvider>();
            var typeActivator = new Mock<ITypeActivator>();
            typeActivator
                .Setup(t => t.CreateInstance(serviceProvider, It.IsAny<Type>(), It.IsAny<object[]>()))
                .Returns((IServiceProvider sp, Type t, object[] args) => Activator.CreateInstance(t));
            var binders = new IModelBinder[]
            {
                new TypeMatchModelBinder(),
                new ByteArrayModelBinder(),
                new GenericModelBinder(serviceProvider, typeActivator.Object),
                new ComplexModelDtoModelBinder(),
                new TypeConverterModelBinder(),
                new MutableObjectModelBinder()
            };

            var binder = new CompositeModelBinder(binders);
            return binder;
        }

        private static CompositeModelBinder CreateCompositeBinder(IModelBinder mockIntBinder)
        {
            var shimBinder = new CompositeModelBinder(new[] { mockIntBinder });
            return shimBinder;
        }

        private static IModelValidatorProvider GetValidatorProvider(params IModelValidator[] validators)
        {
            var provider = new Mock<IModelValidatorProvider>();
            provider.Setup(v => v.GetValidators(It.IsAny<ModelMetadata>()))
                    .Returns(validators ?? Enumerable.Empty<IModelValidator>());

            return provider.Object;
        }

        private class SimplePropertiesModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private sealed class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }

            public List<Person> Friends { get; set; }

            public byte[] Resume { get; set; }
        }

        private class User : IValidatableObject
        {
            public string Password { get; set; }

            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (Password == "password")
                {
                    yield return new ValidationResult("Password does not meet complexity requirements.");
                }
            }
        }
    }
}
#endif
