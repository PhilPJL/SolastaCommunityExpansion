﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionFightingStyleChoiceBuilder : FeatureDefinitionBuilder<FeatureDefinitionFightingStyleChoice, FeatureDefinitionFightingStyleChoiceBuilder>
    {
        #region Constructors
        protected FeatureDefinitionFightingStyleChoiceBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
        {
        }

        protected FeatureDefinitionFightingStyleChoiceBuilder(string name, string definitionGuid) : base(name, definitionGuid)
        {
        }

        protected FeatureDefinitionFightingStyleChoiceBuilder(FeatureDefinitionFightingStyleChoice original, string name, Guid namespaceGuid) : base(original, name, namespaceGuid)
        {
        }

        protected FeatureDefinitionFightingStyleChoiceBuilder(FeatureDefinitionFightingStyleChoice original, string name, string definitionGuid) : base(original, name, definitionGuid)
        {
        }
        #endregion

        public FeatureDefinitionFightingStyleChoiceBuilder ClearFightingStyles()
        {
            Definition.ClearFightingStyles();
            return This();
        }

        public FeatureDefinitionFightingStyleChoiceBuilder SetFightingStyles(params string[] styles)
        {
            return SetFightingStyles(styles.AsEnumerable());
        }

        public FeatureDefinitionFightingStyleChoiceBuilder SetFightingStyles(IEnumerable<string> styles)
        {
            Definition.SetFightingStyles(styles);
            Definition.FightingStyles.Sort();
            return This();
        }

        public FeatureDefinitionFightingStyleChoiceBuilder AddFightingStyles(params string[] styles)
        {
            return AddFightingStyles(styles.AsEnumerable());
        }

        public FeatureDefinitionFightingStyleChoiceBuilder AddFightingStyles(IEnumerable<string> styles)
        {
            Definition.AddFightingStyles(styles);
            Definition.FightingStyles.Sort();
            return This();
        }
    }
}
