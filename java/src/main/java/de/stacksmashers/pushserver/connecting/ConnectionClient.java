package de.stacksmashers.pushserver.connecting;

import com.amazonaws.services.iot.client.*;
import com.amazonaws.services.iot.client.sample.sampleUtil.SampleUtil;
import de.stacksmashers.pushserver.config.Configuration;
import de.stacksmashers.pushserver.config.ConfigurationLoader;
import de.stacksmashers.pushserver.message.LoggingIotMessage;

/**
 * This class handles the connection to AWS using the official AWS libraries
 */
public class ConnectionClient {


    private static ConnectionClient instance;
    private final Configuration configuration;
    private final AWSIotMqttClient client;
    private final ConnectionHistory connectionHistory = new ConnectionHistory();


    private ConnectionClient() {
        configuration = new ConfigurationLoader().loadConfig();

        if (configuration.useCertificate()) {
            String clientEndpoint = configuration.praefix() + ".iot." + configuration.awsRegion() + ".amazonaws.com";       // replace <prefix> and <region> with your own
            String clientId = configuration.clientId();                           // replace with your own client ID. Use unique client IDs for concurrent connections.
            String certificateFile = configuration.certificate();
            String privateKeyFile = configuration.privateKey();
            SampleUtil.KeyStorePasswordPair pair = SampleUtil.getKeyStorePasswordPair(certificateFile, privateKeyFile);
            client = new AWSIotMqttClient(clientEndpoint, clientId, pair.keyStore, pair.keyPassword);
        } else {
            String clientEndpoint = configuration.praefix() + ".iot." + configuration.awsRegion() + ".amazonaws.com";       // replace <prefix> and <region> with your own
            String clientId = configuration.clientId();                           // replace with your own client ID. Use unique client IDs for concurrent connections.
            // String certificateFile = "<certificate file>";                       // X.509 based certificate file
            // String privateKeyFile = "<private key file>";                        // PKCS#1 or PKCS#8 PEM encoded private key file
            String awsKeyId = configuration.awsKeyId();
            String awsSecretkey = configuration.awsSecretKey();
            client = new AWSIotMqttClient(clientEndpoint, clientId, awsKeyId, awsSecretkey);
        }

    }

    public static ConnectionClient getInstance() {
        if (instance == null) {
            instance = new ConnectionClient();
        }
        return instance;
    }

    public void push(String topic, String message) throws AWSIotException {
        connectionHistory.add(System.currentTimeMillis(), "PUBLISH on topic: \"" + topic + "\" with payload: \"" + message + "\"");

        if (client.getConnectionStatus() == AWSIotConnectionStatus.DISCONNECTED) {
            client.connect();
        }
        client.publish(new LoggingIotMessage(topic, AWSIotQos.valueOf(configuration.qos()), message));
    }


    public void subscribe(AWSIotTopic awsIotTopic) throws AWSIotException {
        connectionHistory.add(System.currentTimeMillis(), "SUBSCRIBE on topic: \"" + awsIotTopic.getTopic() + "\"");

        if (client.getConnectionStatus() == AWSIotConnectionStatus.DISCONNECTED) {
            client.connect();
        }
        client.subscribe(awsIotTopic);
    }


}
