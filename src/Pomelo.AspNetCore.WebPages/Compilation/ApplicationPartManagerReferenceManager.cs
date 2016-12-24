// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.CodeAnalysis;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public class ApplicationPartManagerReferenceManager : ReferenceManager
    {
        private readonly ApplicationPartManager _partManager;
        private MetadataReference[] _references;

        public ApplicationPartManagerReferenceManager(ApplicationPartManager partManager)
        {
            _partManager = partManager;
        }

        public override IReadOnlyList<MetadataReference> References
        {
            get
            {
                if (_references == null)
                {
                    var feature = new MetadataReferenceFeature();
                    _partManager.PopulateFeature(feature);
                    _references = feature.MetadataReferences.ToArray();
                }
                
                return _references;
            }
        }
    }
}
