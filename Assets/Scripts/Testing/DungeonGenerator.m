clc;clear;clf;close all;

figure('units','normalized','outerposition',[0 0 1 1])
nPlotsX = 1;
nPlotsY = 1;
plotIdx = 0;
for plotY=1:nPlotsY
    for plotX=1:nPlotsX
        % Starting parameters
        startFacing = deg2rad(RandomBetween(0,360));	% rad
        origin = [0; 0];

        % Path parameters -------------------------------------------------
        depth = 50*plotY;           % meters
        bearing = deg2rad(15);      % rad/meter; tangent to path
        inflectionRate = .4;        % percent/meter
        stepSize = .1;              % meters
        
        [essentialPath, essentialInflectionPts] = CreatePath(origin, startFacing, depth, bearing, inflectionRate, stepSize);

        % Branch parameters (essential path) ------------------------------
        branchRate = 3;                 % branches/path;
        branchDepth = depth/5;          % meters
        branchBearing = deg2rad(35); % rad/meter; tangent to path
        branchInflectionRate = .4;      % percent/meter
        
        % Compute number of branches
        numBranches = floor(branchRate);
        if (rand < mod(branchRate,1))
            numBranches = numBranches + 1;
        end
        
        % Create branches
        branches = zeros(2, round(branchDepth * 1/stepSize), numBranches);
        branchInflectionPoints = cell(1, numBranches);
        branchOriginPts = zeros(size(essentialInflectionPts, 1), numBranches);
        for branchIdx=1:numBranches
            % Choose random inflection point to branch from.
            numEPts = size(essentialInflectionPts, 2);
            branchOriginPts(:, branchIdx) = essentialInflectionPts(:,round(RandomBetween(1, numEPts)));
            deflection = deg2rad(RandomBetween(15,100));    % rad
            deflection = branchOriginPts(4, branchIdx) * deflection + branchOriginPts(3, branchIdx);
            
            % Create branch.
            [branches(:,:,branchIdx), branchInflectionPoints{1, branchIdx}] = CreatePath(branchOriginPts(1:2, branchIdx), deflection, branchDepth, branchBearing, branchInflectionRate, stepSize);
        end
        
        % Plot resulting path. --------------------------------------------
        plotIdx = plotIdx + 1;
        subplot(nPlotsY, nPlotsX, plotIdx);
        
        % essential path
        plot(essentialPath(1,:), essentialPath(2,:), '-r', 'LineWidth', 2);
        hold on
        % branches
        for branchIdx=1:numBranches
           plot(branches(1,:,branchIdx), branches(2,:,branchIdx), '--b'); 
        end
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
