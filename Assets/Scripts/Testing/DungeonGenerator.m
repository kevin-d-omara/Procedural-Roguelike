clc;clear;clf;close all;

% Starting parameters
start = [0, 0];
startAngle = deg2rad(45);   % rad

% Path parameters
depth = 50;        % meters
bendAngle = deg2rad(1); % rad; tangent to path
stepSize = .1;          % meters
inflectionRate = .05;   % percent

% Compute essential path.
numSteps = round(depth * 1/stepSize);
pathX = zeros(1, numSteps);
pathY = zeros(1, numSteps);
theta = zeros(1, numSteps);
stepL = zeros(1, numSteps);
% inflectionPts = cell(1, numSteps);
pathX(1) = start(1);
pathY(1) = start(2);
theta(1) = startAngle;
stepL(1) = 0;
for i=2:numSteps
    if (rand < inflectionRate)
        bendAngle = -bendAngle;
    end
    
    theta(i) = theta(i-1) + bendAngle;
    pathX(i) = stepSize * cos(theta(i)) + pathX(i-1);
    pathY(i) = stepSize * sin(theta(i)) + pathY(i-1);
    
    dx = pathX(i) - pathX(i-1);
    dy = pathY(i) - pathY(i-1);
    stepL(i) = sqrt(dx^2 + dy^2);
end

% Plot essential path.
figure('units','normalized','outerposition',[0 0 1 1])
plot(pathX, pathY, 'b');
hold on
plot(start(1), start(2), 'or');
title({'Essential Path', strcat('depth = ', num2str(depth), 'm'),...
    strcat('bend = ', num2str(rad2deg(bendAngle)), '�')});
bounds = 50;
axis('equal');
axis([-bounds, bounds, -bounds, bounds]);

% % Format display window.
% pos = get(gcf, 'Position');
% % pos(1) = pos(1) * .75
% pos(2) = pos(2) * .66;
% % pos(3:4) = pos(3:4) * 1.25;
% set(gcf, 'position', pos);