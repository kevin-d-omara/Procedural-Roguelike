clc;clear;clf;close all;

% Starting parameters
startAngle = deg2rad(360*rand);   % rad

% Path parameters
depth = 50;        % meters
bendAngle = deg2rad(25);    % rad/meter; tangent to path
inflectionRate = .5;       % percent/meter
stepSize = .1;              % meters

figure('units','normalized','outerposition',[0 0 1 1])
nPlotsX = 4;
nPlotsY = 2;
for plotIdx=1:nPlotsX * nPlotsY
    % Compute essential path.
    path = zeros(2, round(depth * 1/stepSize));
    path(1,1) = 0; path(2,1) = 0;
    inflectionChance = inflectionRate * stepSize;
    dTheta = bendAngle * stepSize;
    theta = startAngle;
    for i=2:round(depth * 1/stepSize)
        if (rand < inflectionChance)
            dTheta = -dTheta;
        end

        theta = theta + dTheta;
        path(1,i) = stepSize * cos(theta) + path(1,i-1);
        path(2,i) = stepSize * sin(theta) + path(2,i-1);
    end

    % Plot essential path.
    subplot(nPlotsY, nPlotsX, plotIdx);
    plot(path(1,:), path(2,:), 'b');
    hold on
    plot(path(1,1), path(2,1), 'or');
    title({'Essential Path', strcat('depth = ', num2str(depth), 'm'),...
        strcat('bend = ', num2str(rad2deg(bendAngle)), '°'),...
        strcat('inflection = ', num2str(inflectionRate*100), '%/m')});
    bounds = 35;
    axis('equal');
    axis([-bounds, bounds, -bounds, bounds]);
end
