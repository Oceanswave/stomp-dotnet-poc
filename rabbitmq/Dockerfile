FROM rabbitmq:3.7-management

RUN rabbitmq-plugins enable --offline rabbitmq_mqtt rabbitmq_stomp rabbitmq_web_stomp

# amqp Port
EXPOSE 5672

# RabbitMQ Management Port
EXPOSE 15672

# STOMP port
EXPOSE 61613