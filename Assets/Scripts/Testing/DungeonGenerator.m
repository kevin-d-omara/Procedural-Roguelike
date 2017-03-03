clc;clear;clf;close all;

figure('units','normalized','outerposition',[0 0 1 1])
nPlotsX = 4;
nPlotsY = 2;
plotIdx = 0;
for plotY=1:nPlotsY
    for plotX=1:nPlotsX
        % Starting parameters
        startFacing = deg2rad(RandomBetween(0,360));	% rad

        % Path parameters
        depth = 50*plotY;           % meters
        bearing = deg2rad(15);    % rad/meter; tangent to path
        inflectionRate = .4;        % percent/meter
        stepSize = .1;              % meters
        
        [essentialPath, essentialInflectionPts] = CreatePath([0; 0], startFacing, depth, bearing, inflectionRate, stepSize);

        % Branch parameters
        numBranches = 2.5;          % branches/path;
        deflection = deg2rad(RandomBetween(30,150));    % rad
        
        % Plot resulting path.
        plotIdx = plotIdx + 1;
        subplot(nPlotsY, nPlotsX, plotIdx);
        
        % essential path
        plot(essentialPath(1,:), essentialPath(2,:), 'b', 'LineWidth', 2);
        hold on
        % origin
        plot(essentialPath(1,1), essentialPath(2,1), 'or');
        % inflection points
        plot(essentialInflectionPts(1,:), essentialInflectionPts(2,:), 'og', 'MarkerFaceColor', 'g', 'MarkerSize', 3);
        
        
        title({'Essential Path', strcat('depth = ', num2str(depth), 'm'),...
            strcat('bend = ', num2str(rad2deg(bearing)), '°'),...
            strcat('inflection = ', num2str(inflectionRate*100), '%/m')});
        bounds = 30;
        axis('equal');
        axis([-bounds, bounds, -bounds, bounds]);
    end
end
