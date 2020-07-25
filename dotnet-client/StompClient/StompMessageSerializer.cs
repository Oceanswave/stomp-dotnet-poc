namespace dotnet_core_client.StompClient
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a serializer that serializes/deserializes a StompFrame to/from a over-the-wire representation.
    /// </summary>
    public class StompMessageSerializer
    {
        /// <summary>
        /// Serializes the frame into a STOMP protocol compatible string.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Serialize(StompFrame message)
        {
            var buffer = new StringBuilder();

            buffer.Append(message.FrameKind + "\n");

            if (message.Headers != null)
            {
                foreach (var header in message.Headers)
                {
                    buffer.Append(header.Key + ":" + header.Value + "\n");
                }
            }

            buffer.Append("\n");
            buffer.Append(message.Body);
            buffer.Append('\0');

            return buffer.ToString();
        }

        /// <summary>
        /// Deserializes the STOMP frame into a StompFrame object.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public StompFrame Deserialize(string message)
        {
            var reader = new StringReader(message);

            var command = reader.ReadLine();

            var headers = new Dictionary<string, string>();

            var header = reader.ReadLine();
            while (!string.IsNullOrEmpty(header))
            {
                var split = header.Split(':');
                if (split.Length == 2) headers[split[0].Trim()] = split[1].Trim();
                header = reader.ReadLine() ?? string.Empty;
            }

            var body = reader.ReadToEnd();
            body = body.TrimEnd('\r', '\n', '\0');

            return new StompFrame(command, body, headers);
        }
    }
}
