using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Tests.Common.TestScenarioGeneration.ScenarioComposition
{
    internal abstract class CompositeScenario : IConfigurationScenario
    {
        private readonly IConfigurationScenario[] _scenarios;

        public virtual string Name { get; }
        public IEnumerable<string> Categories { get; }
        public IEnumerable<IConfigurationScenario> ComposedOf => _scenarios.SelectMany(s => s.ComposedOf).Union(new[] {this});

        protected virtual IEnumerable<string> AdditionalCategories => Enumerable.Empty<string>();

        private static readonly string[] _andCategories = {"SmokeTest"};

        protected CompositeScenario(params IConfigurationScenario[] scenarios)
        {
            _scenarios = scenarios;
            Name = string.Join(".", new[] {GetType().Name}.Union(ComposedOf.Select(s => s.Name)));

            var categories = scenarios
                .SelectMany(s => s.Categories)
                .Union(AdditionalCategories)
                .ToArray();

            var normalCategories = categories
                .Except(_andCategories)
                .ToArray();

            var additionalCategories = _andCategories
                .Where(cat => scenarios.All(s => s.Categories.Contains(cat)))
                .ToArray();

            Categories = new string[0]
                .Union(normalCategories)
                .Union(additionalCategories)
                .ToArray();
        }
    }
}