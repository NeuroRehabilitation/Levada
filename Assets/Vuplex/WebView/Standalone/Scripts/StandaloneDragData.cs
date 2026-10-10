// Copyright (c) 2026 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// Drag data for StandaloneWebView.DragDataChanged.
    /// </summary>
    public class StandaloneDragData {

        internal StandaloneDragData(DragDataChangedMessage message) {

            HasImage = message.HasImage;
            ImageWidth = message.ImageWidth;
            ImageHeight = message.ImageHeight;
            Image = message.Base64EncodedImage == null ? null : System.Convert.FromBase64String(message.Base64EncodedImage);
        }

        /// <summary>
        /// Indicates whether a thumbnail image representation is available for the drag interaction. If `true`,
        /// the application can access the image via the Image field.
        /// </summary>
        public readonly bool HasImage;

        /// <summary>
        /// The width of Image in pixels, or `0` if HasImage is `false`.
        /// </summary>
        public readonly int ImageWidth;

        /// <summary>
        /// The height of Image in pixels, or `0` if HasImage is `false`.
        /// </summary>
        public readonly int ImageHeight;

        /// <summary>
        /// A thumbnail image for the dragged item in PNG format if HasImage is `true`, or `null` otherwise.
        /// </summary>
        public readonly byte[] Image;

        public override string ToString() => $"HasImage: {HasImage}, ImageWidth: {ImageWidth}, ImageHeight: {ImageHeight}, Image: ({Image.Length} bytes)";

        internal static StandaloneDragData FromJson(string messageJson) {

            if (messageJson.Length == 0) {
                return null;
            }
            var message = JsonUtility.FromJson<DragDataChangedMessage>(messageJson);
            return new StandaloneDragData(message);
        }
    }
}
