/*
Copyright 2025 Ho Tzin Mein

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using ChromaFx.Filters.Pipelines.BaseClasses;

namespace ChromaFx;

/// <summary>
/// Generic processing pipeline
/// </summary>
/// <seealso cref="ProcessingPipelineBaseClass"/>
public class ProcessingPipeline : ProcessingPipelineBaseClass
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessingPipeline"/> class.
    /// </summary>
    /// <param name="combine">
    /// if set to <c>true</c> [combine] the convolution filters when possible.
    /// </param>
    public ProcessingPipeline(bool combine)
        : base(combine)
    {
    }
}